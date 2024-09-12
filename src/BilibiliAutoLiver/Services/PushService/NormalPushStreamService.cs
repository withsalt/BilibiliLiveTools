using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Models.Settings;
using BilibiliAutoLiver.Plugin.Base;
using BilibiliAutoLiver.Services.Base;
using BilibiliAutoLiver.Services.FFMpeg.SourceReaders;
using BilibiliAutoLiver.Services.Interface;
using BilibiliAutoLiver.Utils;
using FFMpegCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BilibiliAutoLiver.Services.PushService
{
    public class NormalPushStreamService : BasePushStreamService, INormalPushStreamService
    {
        private readonly ILogger<AdvancePushStreamService> _logger;
        private readonly IBilibiliAccountApiService _account;
        private readonly IBilibiliLiveApiService _api;
        private readonly IFFMpegService _ffmpeg;
        private readonly IPipeContainer _pipeContainer;
        private readonly AppSettings _appSettings;

        private CancellationTokenSource _tokenSource;
        private Task _mainTask;
        private Action _cancel = null;
        private readonly static object _locker = new object();

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
            _account = account ?? throw new ArgumentNullException(nameof(account));
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _ffmpeg = ffmpeg ?? throw new ArgumentNullException(nameof(ffmpeg));
            _pipeContainer = pipeContainer ?? throw new ArgumentNullException(nameof(pipeContainer));
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
                    _cancel?.Invoke();
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
                _cancel = null;

                Status = PushStatus.Stopped;
            }
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
                SettingDto setting = await GetSetting();
                ISourceReader sourceReader = null;
                try
                {
                    //check network
                    while (!await NetworkUtil.Ping())
                    {
                        _logger.LogWarning($"网络连接已断开，将在10秒后重新检查网络连接...");
                        await Task.Delay(10000, _tokenSource.Token);
                    }
                    //start live
                    string rtmpAddr = await GetRtmpAddress();
                    sourceReader = await GetSourceReader(rtmpAddr);
                    FFMpegArgumentProcessor processor = sourceReader
                        .WithInputArg()
                        .WithOutputArg()
                        .CancellableThrough(out _cancel);

                    _logger.LogInformation($"ffmpeg推流命令：{_ffmpeg.GetBinaryPath()} {processor.Arguments}");
                    _logger.LogInformation("推流参数初始化完成");
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
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"推流过程中发生错误，{ex.Message}");
                    SourceReaderDispose(sourceReader);
                    //如果开启了自动重试
                    if (setting.PushSetting.IsAutoRetry && !_tokenSource.IsCancellationRequested)
                    {
                        Delay(setting.PushSetting.RetryInterval, _tokenSource);
                    }
                }
                finally
                {
                    SourceReaderDispose(sourceReader);
                }
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

        /// <summary>
        /// 初始化推流
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetRtmpAddress()
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
            if (liveRoomInfo.area_v2_id != setting.LiveSetting.AreaId || liveRoomInfo.title != setting.LiveSetting.RoomName)
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
            return url;
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
