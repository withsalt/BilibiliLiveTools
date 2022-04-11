using BilibiliLiver.Config.Models;
using BilibiliLiver.Extensions;
using BilibiliLiver.Model;
using BilibiliLiver.Services.Interface;
using BilibiliLiver.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLiver.Services
{
    public class PushStreamService : IPushStreamService
    {
        private readonly ILogger<PushStreamService> _logger;
        private readonly IBilibiliAccountService _account;
        private readonly IBilibiliLiveApiService _api;
        private readonly LiveSetting _liveSetting;

        public PushStreamService(ILogger<PushStreamService> logger
            , IBilibiliAccountService account
            , IBilibiliLiveApiService api
            , IOptions<LiveSetting> liveSettingOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _account = account ?? throw new ArgumentNullException(nameof(account));
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _liveSetting = liveSettingOptions.Value ?? throw new ArgumentNullException(nameof(liveSettingOptions));
        }

        /// <summary>
        /// 测试FFmpeg
        /// </summary>
        /// <returns></returns>
        public async Task<bool> FFmpegTest()
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
                        return true;
                    }
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 开始推流
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<bool> StartPush()
        {
            ProcessStartInfo psi = null;
            bool isAutoRestart = true;
            while (isAutoRestart)
            {
                isAutoRestart = _liveSetting.AutoRestart;
                //check network
                while (!await NetworkUtil.NetworkCheck())
                {
                    _logger.LogWarning($"网络连接已断开，将在10秒后重新检查网络连接...");
                    await Task.Delay(10000);
                }
                //start live
                psi = await InitLiveProcessStartInfo(true);
                if (psi == null)
                {
                    _logger.LogError($"初始化直播参数失败。");
                    return false;
                }
                _logger.LogInformation("正在初始化推流指令...");
                //启动
                using (var proc = Process.Start(psi))
                {
                    if (proc == null || proc.Id <= 0)
                    {
                        throw new Exception("无法执行指定的推流指令，请检查FFmpegCmd是否填写正确。");
                    }
                    await proc.WaitForExitAsync();
                    if (isAutoRestart)
                    {
                        _logger.LogWarning($"FFmpeg异常退出，将在60秒后重试推流。");
                    }
                    else
                    {
                        _logger.LogWarning($"FFmpeg异常退出。");
                    }
                }
                if (isAutoRestart)
                {
                    _logger.LogWarning($"等待重新推流...");
                    //如果开启了自动重试，那么等待60s后再次尝试
                    await Task.Delay(60000);
                }
            }
            return true;
        }


        private async Task<ProcessStartInfo> InitLiveProcessStartInfo(bool isRestart = false)
        {
            try
            {
                //检查Cookie是否有效
                UserInfo userInfo = await _account.Login();
                if (userInfo == null || !userInfo.IsLogin)
                {
                    throw new Exception("登录失败，Cookie已失效");
                }
                //获取直播间信息
                var liveRoomInfo = await _api.GetLiveRoomInfo();
                //如果在直播中，先关闭直播间
                if (liveRoomInfo.live_status == 1 && !isRestart)
                {
                    _logger.LogInformation("当前直播间已经开启，关闭直播间中...");
                    await _api.StopLive(liveRoomInfo.room_id);
                    _logger.LogInformation("直播间已关闭！");
                }
                //开启直播
                StartLiveInfo startLiveInfo = await _api.StartLive(liveRoomInfo.room_id, _liveSetting.LiveAreaId);
                string url = startLiveInfo.rtmp.addr + startLiveInfo.rtmp.code;
                if (string.IsNullOrWhiteSpace(url))
                {
                    _logger.ThrowLogError("获取推流地址失败，请重试！");
                }
                _logger.LogInformation($"获取推流地址成功，推流地址：{url}");

                string newCmd = _liveSetting.FFmpegCmd.Replace("[[URL]]", $"\"{url}\"");
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化直播参数时遇到错误。");
                return null;
            }
        }
    }
}
