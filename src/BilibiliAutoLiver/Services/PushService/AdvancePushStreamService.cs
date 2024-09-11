using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models;
using BilibiliAutoLiver.Config;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Models.Settings;
using BilibiliAutoLiver.Services.Base;
using BilibiliAutoLiver.Services.Interface;
using BilibiliAutoLiver.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BilibiliAutoLiver.Services.PushService
{
    public class AdvancePushStreamService : BasePushStreamService, IAdvancePushStreamService
    {
        private readonly ILogger<AdvancePushStreamService> _logger;
        private readonly IBilibiliAccountApiService _account;
        private readonly IBilibiliLiveApiService _api;
        private readonly IFFMpegService _ffmpeg;
        private readonly AppSettings _appSettings;

        private CancellationTokenSource _tokenSource;
        private Task _mainTask;
        private readonly static object _locker = new object();

        public AdvancePushStreamService(ILogger<AdvancePushStreamService> logger
            , IBilibiliAccountApiService account
            , IBilibiliLiveApiService api
            , IFFMpegService ffmpeg
            , IServiceProvider serviceProvider
            , IOptions<AppSettings> settingOptions) : base(logger, account, api, serviceProvider, ffmpeg, settingOptions.Value)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _account = account ?? throw new ArgumentNullException(nameof(account));
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _ffmpeg = ffmpeg ?? throw new ArgumentNullException(nameof(ffmpeg));
            _appSettings = settingOptions?.Value ?? throw new ArgumentNullException(nameof(settingOptions));
        }

        /// <summary>
        /// 开始推流
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public override async Task<bool> Start()
        {
            if (_mainTask != null)
            {
                if (!await Stop())
                {
                    throw new Exception("停止推流失败！");
                }
            }
            if (_tokenSource != null)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();
                _tokenSource = null;
            }
            Status = PushStatus.Starting;
            _tokenSource = new CancellationTokenSource();
            _mainTask = Task.Run(PushStream);
            return true;
        }

        /// <summary>
        /// 停止推流
        /// </summary>
        /// <returns></returns>
        public override Task<bool> Stop()
        {
            try
            {
                if (_mainTask == null)
                {
                    return Task.FromResult(true);
                }
                if (_tokenSource == null || _tokenSource.IsCancellationRequested)
                {
                    return Task.FromResult(true);
                }
                lock (_locker)
                {
                    _logger.LogWarning("结束推流中...");
                    _tokenSource.Cancel();
                    Stopwatch sw = Stopwatch.StartNew();
                    //3s等待下线
                    while (sw.ElapsedMilliseconds < 3000 && (_mainTask.Status == TaskStatus.Running || _mainTask.Status == TaskStatus.WaitingForActivation || _mainTask.Status == TaskStatus.WaitingToRun))
                    {
                        Thread.Sleep(0);
                    }
                    sw.Stop();
                    if (_mainTask.Status != TaskStatus.RanToCompletion)
                    {
                        return Task.FromResult(false);
                    }
                    _logger.LogWarning("推流已停止。");
                }
                return Task.FromResult(true);
            }
            finally
            {
                //Dispose
                _mainTask?.Dispose();
                _tokenSource?.Dispose();
                _mainTask = null;
                _tokenSource = null;

                Status = PushStatus.Stopped;
            }
        }

        /// <summary>
        /// 初始化推流
        /// </summary>
        /// <returns></returns>
        private async Task<ProcessStartInfo> BuildProcessStartInfo()
        {
            SettingDto setting = await GetSetting();
            //检查Cookie是否有效
            UserInfo userInfo = await _account.LoginByCookie();
            if (userInfo == null || !userInfo.IsLogin)
            {
                throw new Exception("登录失败，Cookie已失效");
            }
            //获取直播间信息
            var liveRoomInfo = await _api.GetMyLiveRoomInfo();
            if (liveRoomInfo.title != setting.LiveSetting.RoomName || liveRoomInfo.area_v2_id != setting.LiveSetting.AreaId)
            {
                await _api.UpdateLiveRoomInfo(liveRoomInfo.room_id, setting.LiveSetting.RoomName, setting.LiveSetting.AreaId);
            }
            //开启直播
            StartLiveInfo startLiveInfo = await _api.StartLive(liveRoomInfo.room_id, setting.LiveSetting.AreaId);
            string url = startLiveInfo.rtmp.addr + startLiveInfo.rtmp.code;
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new Exception("获取推流地址失败，请重试！");
            }
            _logger.LogInformation($"获取推流地址成功，推流地址：{url}");

            if (!CmdAnalyzer.TryParse(setting.PushSetting.FFmpegCommand, _appSettings.AdvanceStrictMode, Path.Combine(_appSettings.DataDirectory, GlobalConfigConstant.DefaultMediaDirectory), url, out string message, out string newCmd, out string cmdName, out string cmdArgs))
            {
                throw new Exception(message);
            }
            if (cmdName.Equals("ffmpeg", StringComparison.OrdinalIgnoreCase) || cmdName.Equals("ffmpeg.exe", StringComparison.OrdinalIgnoreCase))
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
                Status = PushStatus.Starting;
                Process proc = null;
                SettingDto setting = await GetSetting();
                try
                {
                    //check network
                    while (!await NetworkUtil.Ping())
                    {
                        _logger.LogWarning($"网络连接已断开，将在10秒后重新检查网络连接...");
                        await Task.Delay(10000, _tokenSource.Token);
                    }
                    //start live
                    ProcessStartInfo psi = await BuildProcessStartInfo();
                    _logger.LogInformation($"ffmpeg推流命令：{psi.FileName} {psi.Arguments}");
                    _logger.LogInformation("推流参数初始化完成");
                    //启动
                    proc = Process.Start(psi);
                    if (proc == null || proc.Id <= 0)
                    {
                        throw new Exception("无法执行指定的推流指令，请检查FFmpegCmd是否填写正确。");
                    }
                    Status = PushStatus.Running;
                    _logger.LogInformation("开始推流...");
                    await proc.WaitForExitAsync(_tokenSource.Token);
                    proc.Kill();
                    //delay 100ms的原因是ffmpeg本身也会接收ctrl-c，但是C#的控制台要比ffmpeg慢一点。
                    //就导致ffmpeg退出要早一点
                    await Task.Delay(100);
                    if (!_tokenSource.IsCancellationRequested)
                    {
                        _logger.LogWarning($"FFmpeg异常退出。");
                    }

                    //如果开启了自动重试
                    if (setting.PushSetting.IsAutoRetry && !_tokenSource.IsCancellationRequested)
                    {
                        Status = PushStatus.Waiting;
                        Delay(setting.PushSetting.RetryInterval, _tokenSource);
                    }
                }
                catch (OperationCanceledException)
                {
                    try
                    {
                        if (proc != null && !proc.HasExited)
                        {
                            proc.Kill();
                        }
                    }
                    catch { }
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"推流过程中发生错误，{ex.Message}");
                    //如果开启了自动重试
                    if (setting.PushSetting.IsAutoRetry && !_tokenSource.IsCancellationRequested)
                    {
                        Delay(setting.PushSetting.RetryInterval, _tokenSource);
                    }
                }
                finally
                {
                    proc?.Dispose();
                }
            }
        }

        private readonly static object _disposeLock = new object();
        private static bool _disposed = false;

        public override void Dispose()
        {
            if (!_disposed)
            {
                lock (_disposeLock)
                {
                    if (!_disposed)
                    {
                        _disposed = true;
                        this.Stop();
                    }
                }
            }
        }
    }
}
