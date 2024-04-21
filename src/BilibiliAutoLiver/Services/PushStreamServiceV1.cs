using BilibiliAutoLiver.Extensions;
using BilibiliAutoLiver.Models;
using BilibiliAutoLiver.Services.Interface;
using BilibiliAutoLiver.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace BilibiliAutoLiver.Services
{
    public class PushStreamServiceV1 : IPushStreamServiceV1
    {
        private readonly ILogger<PushStreamServiceV1> _logger;
        private readonly IBilibiliAccountService _account;
        private readonly IBilibiliLiveApiService _api;
        private readonly IFFMpegService _ffmpeg;
        private readonly LiveSettings _liveSetting;

        private CancellationTokenSource _tokenSource;
        private Task _mainTask;

        public PushStreamServiceV1(ILogger<PushStreamServiceV1> logger
            , IBilibiliAccountService account
            , IBilibiliLiveApiService api
            , IOptions<LiveSettings> liveSettingOptions
            , IFFMpegService ffmpeg)
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
        public async Task StartPush()
        {
            if (_mainTask != null)
            {
                await StopPush();
            }
            if (_tokenSource != null)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();
                _tokenSource = null;
            }
            _tokenSource = new CancellationTokenSource();
            _mainTask = Task.Run(async () =>
            {
                await PushStream();
            });
        }

        /// <summary>
        /// 停止推流
        /// </summary>
        /// <returns></returns>
        public async Task StopPush()
        {
            if (_mainTask == null)
            {
                return;
            }
            if (_tokenSource == null || _tokenSource.IsCancellationRequested)
            {
                return;
            }
            _logger.LogWarning("结束推流中...");
            _tokenSource.Cancel();
            Stopwatch sw = Stopwatch.StartNew();
            //3s等待下线
            while (sw.ElapsedMilliseconds < 3000
                && (_mainTask.Status == TaskStatus.Running || _mainTask.Status == TaskStatus.WaitingForActivation))
            {
                await Task.Delay(10);
            }
            sw.Stop();
            //Dispose
            _mainTask.Dispose();
            _tokenSource.Dispose();
            _mainTask = null;
            _tokenSource = null;
            _logger.LogWarning("推流中已停止。");
        }

        /// <summary>
        /// 测试FFmpeg
        /// </summary>
        /// <returns></returns>
        public async Task CheckFFmpegBinary()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = @"ffmpeg",
                    Arguments = "-version",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = RuntimeInformation.IsOSPlatform(OSPlatform.Linux),
                };
                using (var proc = Process.Start(psi))
                {
                    if (proc != null && proc.Id > 0)
                    {
                        string result = await proc.StandardOutput.ReadToEndAsync();
                        if (!string.IsNullOrEmpty(result))
                        {
                            string[] allLines = result.Split('\n');
                            string[] versionLine = allLines.Where(p => p.Contains("ffmpeg version", StringComparison.OrdinalIgnoreCase)).ToArray();
                            if (versionLine.Length > 0)
                            {
                                _logger.LogInformation(versionLine[0]);
                            }
                        }
                        proc.Kill();
                        return;
                    }
                }
                throw new Exception("进程启用失败。");
            }
            catch(Exception ex)
            {
                _logger.ThrowLogError($"FFmpeg测试失败，{ex.Message}");
            }
        }

        /// <summary>
        /// 检查配置文件
        /// </summary>
        public Task CheckLiveSetting()
        {
            if (_liveSetting.LiveAreaId <= 0)
            {
                _logger.ThrowLogError("配置文件appsettings.json中，LiveSetting.LiveAreaId填写错误！");
            }
            if (string.IsNullOrWhiteSpace(_liveSetting.LiveRoomName))
            {
                _logger.ThrowLogError("配置文件appsettings.json中，LiveSetting.LiveRoomName不能为空！");
            }
            if (string.IsNullOrWhiteSpace(_liveSetting.FFmpegCmd))
            {
                _logger.ThrowLogError("配置文件appsettings.json中，LiveSetting.FFmpegCmd不能为空！");
            }
            int markIndex = _liveSetting.FFmpegCmd.IndexOf("[[URL]]");
            if (markIndex < 5)
            {
                throw new Exception("配置文件appsettings.json中，LiveSetting.FFmpegCmd不正确，命令中未找到 '[[URL]]'标记。");
            }
            if (_liveSetting.FFmpegCmd[markIndex - 1] == '\"')
            {
                throw new Exception("配置文件appsettings.json中，LiveSetting.FFmpegCmd不正确， '[[URL]]'标记前后无需“\"”。");
            }
            return Task.CompletedTask;
        }


        /// <summary>
        /// 检查直播间信息
        /// </summary>
        public async Task CheckLiveRoom()
        {
            //登录
            var userInfo = await _account.LoginByCookie();
            if (userInfo == null || !userInfo.IsLogin)
            {
                _logger.ThrowLogError("登录失败，Cookie无效或已过期，请重新配置Cookie！");
            }
            _logger.LogInformation($"用户{userInfo.Uname}，登录成功！");
            //获取直播间信息
            var liveRoomInfo = await _api.GetLiveRoomInfo();
            if (liveRoomInfo == null)
            {
                _logger.ThrowLogError("获取直播间信息失败！");
            }
            if (liveRoomInfo.room_id == 0 || liveRoomInfo.have_live == 0)
            {
                _logger.ThrowLogError("当前用户未开通直播间！");
            }
            _logger.LogInformation($"获取直播间信息成功，当前直播间地址：http://live.bilibili.com/{liveRoomInfo.room_id}，名称：{liveRoomInfo.title}，分区：{liveRoomInfo.parent_name}·{liveRoomInfo.area_v2_name}，直播状态：{(liveRoomInfo.live_status == 1 ? "直播中" : "未开启")}");
            //检查名称
            if (liveRoomInfo.title != _liveSetting.LiveRoomName)
            {
                bool result = await _api.UpdateLiveRoomName(liveRoomInfo.room_id, _liveSetting.LiveRoomName);
                if (!result)
                {
                    _logger.ThrowLogError($"修改直播间名称为【{_liveSetting.LiveRoomName}】失败！");
                }
                _logger.LogInformation($"修改直播间名称为：{_liveSetting.LiveRoomName}，成功！");
                await Task.Delay(1000);
            }
            //检查分区
            if (liveRoomInfo.area_v2_id != _liveSetting.LiveAreaId)
            {
                bool result = await _api.UpdateLiveRoomArea(liveRoomInfo.room_id, _liveSetting.LiveAreaId);
                if (!result)
                {
                    _logger.ThrowLogError($"修改直播间分区为【{_liveSetting.LiveAreaId}】失败！");
                }
                _logger.LogInformation($"修改直播间分区为：{_liveSetting.LiveAreaId}，成功！");
                await Task.Delay(1000);
            }
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
            var liveRoomInfo = await _api.GetLiveRoomInfo();
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

            string newCmd = _liveSetting.FFmpegCmd
                .Trim('\r', '\n', ' ')
                .Replace("[[URL]]", $"\"{url}\"");
            int firstNullChar = newCmd.IndexOf((char)32);
            if (firstNullChar < 0)
            {
                throw new Exception("无法获取命令执行名称，比如在命令ffmpeg -version中，无法获取ffmpeg。");
            }
            string cmdName = newCmd.Substring(0, firstNullChar);
            string cmdArgs = newCmd.Substring(firstNullChar);
            if (string.IsNullOrEmpty(cmdArgs))
            {
                throw new Exception("命令参数不能为空！");
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
            bool isAutoRestart = true;
            while (isAutoRestart && !_tokenSource.IsCancellationRequested)
            {
                try
                {
                    isAutoRestart = _liveSetting.AutoRestart;
                    //check network
                    while (!await NetworkUtil.Ping())
                    {
                        _logger.LogWarning($"网络连接已断开，将在10秒后重新检查网络连接...");
                        await Task.Delay(10000, _tokenSource.Token);
                    }
                    //start live
                    ProcessStartInfo psi = await InitLiveProcessStartInfo();
                    _logger.LogInformation("推流参数初始化完成，即将开始推流...");
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
                    if (isAutoRestart && !_tokenSource.IsCancellationRequested)
                    {
                        _logger.LogWarning($"等待60s后重新推流...");
                        await Task.Delay(60000, _tokenSource.Token);
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
                    if (isAutoRestart && !_tokenSource.IsCancellationRequested)
                    {
                        _logger.LogWarning($"等待60s后重新推流...");
                        await Task.Delay(60000, _tokenSource.Token);
                    }
                }
            }
        }

    }
}
