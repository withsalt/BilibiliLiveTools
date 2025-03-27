using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using BilibiliAutoLiver.Config;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Models.Settings;
using BilibiliAutoLiver.Services.Base;
using BilibiliAutoLiver.Services.Interface;
using BilibiliAutoLiver.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BilibiliAutoLiver.Services.FFMpeg.Services.PushService
{
    public class AdvancePushStreamService : BasePushStreamService, IAdvancePushStreamService
    {
        private readonly ILogger<AdvancePushStreamService> _logger;
        private readonly IFFMpegService _ffmpeg;
        private readonly AppSettings _appSettings;

        public AdvancePushStreamService(ILogger<AdvancePushStreamService> logger
            , IBilibiliAccountApiService account
            , IBilibiliLiveApiService api
            , IFFMpegService ffmpeg
            , IServiceProvider serviceProvider
            , IOptions<AppSettings> settingOptions) : base(logger, account, api, serviceProvider, ffmpeg, settingOptions.Value)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ffmpeg = ffmpeg ?? throw new ArgumentNullException(nameof(ffmpeg));
            _appSettings = settingOptions?.Value ?? throw new ArgumentNullException(nameof(settingOptions));
        }

        /// <summary>
        /// 开启推流
        /// </summary>
        /// <returns></returns>
        protected override async Task PushStream()
        {
            try
            {
                while (!_tokenSource.IsCancellationRequested)
                {
                    Status = PushStatus.Starting;
                    Process proc = null;
                    SettingDto setting = await GetSetting();

                    try
                    {
                        //check network
                        await CheckNetwork(_tokenSource);
                        //start live
                        (string cmdName, string cmdArg) = await BuildFFMpegCommand();

                        _logger.LogInformation($"FFMpeg推流命令：{cmdName} {cmdArg}");
                        _logger.LogInformation("推流参数初始化完成");

                        _ffmpeg.AddLog(LogType.Info, $"======================={DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 开始推流====================");
                        _ffmpeg.AddLog(LogType.Info, $"FFMpeg推流命令：{cmdName} {cmdArg}");

                        //启动
                        proc = new Process();
                        proc.StartInfo.FileName = cmdName;
                        proc.StartInfo.Arguments = cmdArg;
                        proc.StartInfo.RedirectStandardOutput = true;
                        proc.StartInfo.RedirectStandardError = true;
                        proc.StartInfo.UseShellExecute = false;
                        proc.StartInfo.CreateNoWindow = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
                        proc.StartInfo.StandardErrorEncoding = Encoding.UTF8;
                        proc.StartInfo.StandardOutputEncoding = Encoding.UTF8;

                        //日志
                        proc.OutputDataReceived += (s, e) =>
                        {
                            if (!string.IsNullOrEmpty(e.Data))
                            {
                                _ffmpeg.AddLog(LogType.Info, e.Data);
                            }
                        };
                        proc.ErrorDataReceived += (s, e) =>
                        {
                            if (!string.IsNullOrEmpty(e.Data))
                            {
                                _ffmpeg.AddLog(LogType.Error, e.Data);
                            }
                        };

                        bool isStart = proc.Start();
                        if (!isStart)
                        {
                            throw new Exception("无法执行指定的推流指令，请检查FFmpegCmd是否填写正确。");
                        }
                        // 开始异步读取输出
                        proc.BeginOutputReadLine();
                        proc.BeginErrorReadLine();

                        Status = PushStatus.Running;
                        _logger.LogInformation("开始推流...");

                        await proc.WaitForExitAsync(_tokenSource.Token);
                        proc.Kill();
                        proc.Dispose();

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
                        else
                        {
                            _logger.LogInformation("未开启不间断直播，直播停止");
                            _ffmpeg.AddLog(LogType.Info, $"未开启不间断直播，直播停止");
                            break;
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
                        _ffmpeg.AddLog(LogType.Error, ex.Message, ex);
                        //如果开启了自动重试
                        if (setting.PushSetting.IsAutoRetry && !_tokenSource.IsCancellationRequested)
                        {
                            Delay(setting.PushSetting.RetryInterval, _tokenSource);
                        }
                        else
                        {
                            _logger.LogInformation("未开启不间断直播，直播停止");
                            _ffmpeg.AddLog(LogType.Info, $"未开启不间断直播，直播停止");
                            break;
                        }
                    }
                    finally
                    {
                        proc?.Dispose();
                    }
                }
            }
            finally
            {
                //终止后设置状态为终止
                Status = PushStatus.Stopped;
            }
        }

        /// <summary>
        /// 初始化推流
        /// </summary>
        /// <returns></returns>
        private async Task<(string cmdName, string cmdArg)> BuildFFMpegCommand()
        {
            SettingDto setting = await GetSetting();
            string url = await GetRtmpAddress();
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new Exception("获取推流地址失败，请重试！");
            }
            if (!CmdAnalyzer.TryParse(setting.PushSetting.FFmpegCommand, _appSettings.AdvanceStrictMode, Path.Combine(_appSettings.DataDirectory, GlobalConfigConstant.DefaultMediaDirectory), url, out string message, out string newCmd, out string cmdName, out string cmdArgs))
            {
                throw new Exception(message);
            }
            if (cmdName.Equals("ffmpeg", StringComparison.OrdinalIgnoreCase) || cmdName.Equals("ffmpeg.exe", StringComparison.OrdinalIgnoreCase))
            {
                cmdName = _ffmpeg.GetBinaryPath();
            }
            return (cmdName, cmdArgs);
        }
    }
}
