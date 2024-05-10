using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Models.Settings;
using BilibiliAutoLiver.Plugin.Base;
using BilibiliAutoLiver.Services.Base;
using BilibiliAutoLiver.Services.FFMpeg.DeviceProviders;
using BilibiliAutoLiver.Services.FFMpeg.SourceReaders;
using BilibiliAutoLiver.Services.Interface;
using BilibiliAutoLiver.Utils;
using FFMpegCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BilibiliAutoLiver.Services
{
    public class PushStreamServiceV2 : BasePushStreamService, IPushStreamServiceV2
    {
        private readonly ILogger<PushStreamServiceV1> _logger;
        private readonly IBilibiliAccountApiService _account;
        private readonly IBilibiliLiveApiService _api;
        private readonly IFFMpegService _ffmpeg;
        private readonly LiveSettings _liveSetting;
        private readonly IPipeContainer _pipeContainer;

        private CancellationTokenSource _tokenSource;
        private Task _mainTask;
        private Action _cancel = null;

        public PushStreamServiceV2(ILogger<PushStreamServiceV1> logger
            , IBilibiliAccountApiService account
            , IBilibiliLiveApiService api
            , IOptions<LiveSettings> liveSettingOptions
            , IFFMpegService ffmpeg
            , IPipeContainer pipeContainer) : base(logger, account, api, liveSettingOptions, ffmpeg)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _account = account ?? throw new ArgumentNullException(nameof(account));
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _ffmpeg = ffmpeg ?? throw new ArgumentNullException(nameof(ffmpeg));
            _liveSetting = liveSettingOptions.Value ?? throw new ArgumentNullException(nameof(liveSettingOptions));
            _pipeContainer = pipeContainer ?? throw new ArgumentNullException(nameof(pipeContainer));
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
        public override async Task Stop()
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
            _cancel?.Invoke();
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
            _cancel = null;
            _logger.LogWarning("推流中已停止。");
        }

        /// <summary>
        /// 开启推流
        /// </summary>
        /// <returns></returns>
        private async Task PushStream()
        {
            while (!_tokenSource.IsCancellationRequested)
            {
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
                    sourceReader = GetSourceReader(rtmpAddr);
                    FFMpegArgumentProcessor processor = sourceReader.WithInputArg().WithOutputArg()
                        .CancellableThrough(out _cancel);

                    _logger.LogInformation($"ffmpeg推流命令：{_ffmpeg.GetBinaryPath()} {processor.Arguments}");
                    _logger.LogInformation("推流参数初始化完成，开始推流...");
                    //启动
                    await processor.ProcessAsynchronously();

                    //如果开启了自动重试
                    if (!_tokenSource.IsCancellationRequested)
                    {
                        _logger.LogWarning($"等待{_liveSetting.RetryDelay}s后重新推流...");
                        await Task.Delay(_liveSetting.RetryDelay * 1000, _tokenSource.Token);
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
                        _logger.LogWarning($"等待60s后重新推流...");
                        await Task.Delay(60000, _tokenSource.Token);
                    }
                }
                finally
                {
                    if (sourceReader != null) sourceReader.Dispose();
                }
            }
        }

        private ISourceReader GetSourceReader(string rtmpAddr)
        {
            switch (_liveSetting.V2.Input.VideoSource.Type)
            {
                case InputSourceType.File:
                    return new VideoSourceReader(_liveSetting, rtmpAddr, _logger);
                case InputSourceType.Desktop:
                    return new DesktopSourceReader(_liveSetting, rtmpAddr, _logger);
                case InputSourceType.Device:
                    return new DeviceSourceReader(_liveSetting, rtmpAddr, _logger, _pipeContainer);
                default:
                    throw new NotImplementedException($"不支持的输入类型：{_liveSetting.V2.Input.VideoSource.Type}");
            }
        }

        /// <summary>
        /// 初始化推流
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetRtmpAddress()
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
            return url;
        }
    }
}
