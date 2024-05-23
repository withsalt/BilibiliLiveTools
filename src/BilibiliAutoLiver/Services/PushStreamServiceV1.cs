using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models;
using BilibiliAutoLiver.Models.Settings;
using BilibiliAutoLiver.Services.Base;
using BilibiliAutoLiver.Services.Interface;
using BilibiliAutoLiver.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BilibiliAutoLiver.Services
{
    public class PushStreamServiceV1 : BasePushStreamService, IPushStreamServiceV1
    {
        private readonly ILogger<PushStreamServiceV1> _logger;
        private readonly IBilibiliAccountApiService _account;
        private readonly IBilibiliLiveApiService _api;
        private readonly IFFMpegService _ffmpeg;
        private readonly LiveSettings _liveSetting;

        private CancellationTokenSource _tokenSource;
        private Task _mainTask;
        private readonly static object _locker = new object();

        public PushStreamServiceV1(ILogger<PushStreamServiceV1> logger
            , IBilibiliAccountApiService account
            , IBilibiliLiveApiService api
            , IOptions<LiveSettings> liveSettingOptions
            , IFFMpegService ffmpeg) : base(logger, account, api, liveSettingOptions, ffmpeg)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _account = account ?? throw new ArgumentNullException(nameof(account));
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _ffmpeg = ffmpeg ?? throw new ArgumentNullException(nameof(ffmpeg));
            _liveSetting = liveSettingOptions.Value ?? throw new ArgumentNullException(nameof(liveSettingOptions));
        }

        /// <summary>
        /// 开始推流
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public override async Task Start()
        {
            if (_mainTask != null)
            {
                await Stop();
            }
            if (_tokenSource != null)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();
                _tokenSource = null;
            }
            _tokenSource = new CancellationTokenSource();
            _mainTask = Task.Run(PushStream);
        }

        /// <summary>
        /// 停止推流
        /// </summary>
        /// <returns></returns>
        public override Task Stop()
        {
            if (_mainTask == null)
            {
                return Task.CompletedTask;
            }
            if (_tokenSource == null || _tokenSource.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }
            lock (_locker)
            {
                _logger.LogWarning("结束推流中...");
                _tokenSource.Cancel();
                Stopwatch sw = Stopwatch.StartNew();
                //3s等待下线
                while (sw.ElapsedMilliseconds < 3000 && (_mainTask.Status == TaskStatus.Running || _mainTask.Status == TaskStatus.WaitingForActivation))
                {
                    Thread.Sleep(0);
                }
                sw.Stop();
                //Dispose
                _mainTask.Dispose();
                _tokenSource.Dispose();
                _mainTask = null;
                _tokenSource = null;
                _logger.LogWarning("推流已停止。");
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 初始化推流
        /// </summary>
        /// <returns></returns>
        private async Task<ProcessStartInfo> InitLiveProcessStartInfo()
        {
            //检查Cookie是否有效
            UserInfo userInfo = await _account.LoginByCookie();
            if (userInfo == null || !userInfo.IsLogin)
            {
                throw new Exception("登录失败，Cookie已失效");
            }
            //获取直播间信息
            var liveRoomInfo = await _api.GetMyLiveRoomInfo();
            if (liveRoomInfo.area_v2_id != _liveSetting.LiveAreaId)
            {
                await _api.UpdateLiveRoomArea(liveRoomInfo.room_id, _liveSetting.LiveAreaId);
            }
            if (liveRoomInfo.title != _liveSetting.LiveRoomName)
            {
                await _api.UpdateLiveRoomName(liveRoomInfo.room_id, _liveSetting.LiveRoomName);
            }
            //开启直播
            StartLiveInfo startLiveInfo = await _api.StartLive(liveRoomInfo.room_id, _liveSetting.LiveAreaId);
            string url = startLiveInfo.rtmp.addr + startLiveInfo.rtmp.code;
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new Exception("获取推流地址失败，请重试！");
            }
            _logger.LogInformation($"获取推流地址成功，推流地址：{url}");

            string newCmd = _liveSetting.V1.FFmpegCommands.GetTargetOSPlatformCommand()
                .Trim('\r', '\n', ' ')
                .Replace("[[URL]]", $"\"{url}\"");
            int firstNullChar = newCmd.IndexOf((char)32);
            if (firstNullChar < 0)
            {
                throw new Exception("无法获取命令执行名称，比如在命令ffmpeg -version中，无法获取ffmpeg。");
            }
            string cmdName = newCmd.Substring(0, firstNullChar);
            if (string.IsNullOrEmpty(cmdName))
            {
                throw new Exception("命令名称不能为空！");
            }
            string cmdArgs = newCmd.Substring(firstNullChar);
            if (string.IsNullOrEmpty(cmdArgs))
            {
                throw new Exception("命令参数不能为空！");
            }
            if (cmdName.EndsWith("ffmpeg", StringComparison.OrdinalIgnoreCase) || cmdName.EndsWith("ffmpeg.exe", StringComparison.OrdinalIgnoreCase))
            {
                cmdName = _ffmpeg.GetBinaryPath();
            }
            var psi = new ProcessStartInfo
            {
                FileName = cmdName,
                Arguments = cmdArgs,
                RedirectStandardOutput = true,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = RuntimeInformation.IsOSPlatform(OSPlatform.Linux),
            };
            return psi;
        }

        /// <summary>
        /// 开启推流
        /// </summary>
        /// <returns></returns>
        private async Task PushStream()
        {
            while (!_tokenSource.IsCancellationRequested)
            {
                try
                {
                    //check network
                    while (!await NetworkUtil.Ping())
                    {
                        _logger.LogWarning($"网络连接已断开，将在10秒后重新检查网络连接...");
                        await Task.Delay(10000, _tokenSource.Token);
                    }
                    //start live
                    ProcessStartInfo psi = await InitLiveProcessStartInfo();
                    _logger.LogInformation($"ffmpeg推流命令：{psi.FileName} {psi.Arguments}");
                    _logger.LogInformation("推流参数初始化完成，开始推流...");
                    //启动
                    using (var proc = Process.Start(psi))
                    {
                        if (proc == null || proc.Id <= 0)
                        {
                            throw new Exception("无法执行指定的推流指令，请检查FFmpegCmd是否填写正确。");
                        }
                        await proc.WaitForExitAsync(_tokenSource.Token);
                        proc.Kill();
                        //delay 100ms的原因是ffmpeg本身也会接收ctrl-c，但是C#的控制台要比ffmpeg慢一点。
                        //就导致ffmpeg退出要早一点
                        await Task.Delay(100);
                        if (!_tokenSource.IsCancellationRequested)
                        {
                            _logger.LogWarning($"FFmpeg异常退出。");
                        }
                    }
                    //如果开启了自动重试
                    if (!_tokenSource.IsCancellationRequested)
                    {
                        await Delay(_tokenSource);
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"推流过程中发生错误，{ex.Message}");
                    //如果开启了自动重试
                    if (!_tokenSource.IsCancellationRequested)
                    {
                        await Delay(_tokenSource);
                    }
                }
            }
        }

    }
}
