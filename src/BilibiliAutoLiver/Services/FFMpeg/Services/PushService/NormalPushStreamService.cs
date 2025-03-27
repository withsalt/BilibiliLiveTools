using System;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Models.Settings;
using BilibiliAutoLiver.Plugin.Base;
using BilibiliAutoLiver.Services.Base;
using BilibiliAutoLiver.Services.FFMpeg.SourceReaders;
using BilibiliAutoLiver.Services.Interface;
using FFMpegCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BilibiliAutoLiver.Services.FFMpeg.Services.PushService
{
    public class NormalPushStreamService : BasePushStreamService, INormalPushStreamService
    {
        private readonly ILogger<AdvancePushStreamService> _logger;
        private readonly IFFMpegService _ffmpeg;
        private readonly IPipeContainer _pipeContainer;

        public NormalPushStreamService(ILogger<AdvancePushStreamService> logger
            , IBilibiliAccountApiService account
            , IBilibiliLiveApiService api
            , IFFMpegService ffmpeg
            , IPipeContainer pipeContainer
            , IMemoryCache cache
            , IServiceProvider serviceProvider
            , IOptions<AppSettings> settingOptions) : base(logger, account, api, serviceProvider, ffmpeg, settingOptions.Value)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ffmpeg = ffmpeg ?? throw new ArgumentNullException(nameof(ffmpeg));
            _pipeContainer = pipeContainer ?? throw new ArgumentNullException(nameof(pipeContainer));
        }

        /// <summary>
        /// 开启推流
        /// </summary>
        /// <returns></returns>
        protected override async Task PushStream()
        {
            try
            {
                Guid infoLogGuiid = Guid.NewGuid();
                Guid errorLogGuiid = Guid.NewGuid();

                while (!_tokenSource.IsCancellationRequested)
                {
                    Status = PushStatus.Starting;
                    SettingDto setting = await GetSetting();
                    ISourceReader sourceReader = null;
                    try
                    {
                        //check network
                        await CheckNetwork(_tokenSource);
                        //start live
                        string rtmpAddr = await GetRtmpAddress();
                        sourceReader = await GetSourceReader(rtmpAddr);
                        FFMpegArgumentProcessor processor = sourceReader
                            .WithInputArg()
                            .WithOutputArg()
                            .CancellableThrough(out _cancel);

                        processor.NotifyOnOutput(p =>
                        {
                            _ffmpeg.AddLog(LogType.Info, p);
                        });
                        processor.NotifyOnError(p =>
                        {
                            _ffmpeg.AddLog(LogType.Error, p);
                        });

                        _logger.LogInformation($"FFMpeg推流命令：{_ffmpeg.GetBinaryPath()} {processor.Arguments}");
                        _logger.LogInformation("推流参数初始化完成");

                        _ffmpeg.AddLog(LogType.Info, $"======================={DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 开始推流====================");
                        _ffmpeg.AddLog(LogType.Info, $"FFMpeg推流命令：{_ffmpeg.GetBinaryPath()} {processor.Arguments}");

                        //启动
                        Status = PushStatus.Running;
                        _logger.LogInformation("开始推流...");
                        await processor.ProcessAsynchronously();

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
                        return;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"推流过程中发生错误，{ex.Message}");
                        _ffmpeg.AddLog(LogType.Error, ex.Message, ex);
                        SourceReaderDispose(sourceReader);
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
                        SourceReaderDispose(sourceReader);
                    }
                }
            }
            finally
            {
                //终止后设置状态为终止
                Status = PushStatus.Stopped;
            }
        }

        private void SourceReaderDispose(ISourceReader sourceReader)
        {
            try
            {
                if (sourceReader != null)
                    sourceReader.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"终止{sourceReader.GetType().Name}失败。");
            }
        }

        private async Task<ISourceReader> GetSourceReader(string rtmpAddr)
        {
            SettingDto setting = await GetSetting();
            switch (setting.PushSetting.InputType)
            {
                case InputType.Video:
                    return new VideoSourceReader(setting, rtmpAddr, _logger);
                case InputType.Desktop:
                    return new DesktopSourceReader(setting, rtmpAddr, _logger);
                case InputType.Camera:
                    return new CameraSourceReader(setting, rtmpAddr, _logger);
                case InputType.CameraPlus:
                    return new CameraPlusSourceReader(setting, rtmpAddr, _logger, _pipeContainer);
                default:
                    throw new NotImplementedException($"不支持的输入类型：{setting.PushSetting.InputType}");
            }
        }
    }
}
