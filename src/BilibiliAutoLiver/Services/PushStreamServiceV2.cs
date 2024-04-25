using System;
using System.Threading;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using BilibiliAutoLiver.Models;
using BilibiliAutoLiver.Services.Base;
using BilibiliAutoLiver.Services.Interface;
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

        private CancellationTokenSource _tokenSource;
        private Task _mainTask;

        public PushStreamServiceV2(ILogger<PushStreamServiceV1> logger
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
        public override Task Start()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 停止推流
        /// </summary>
        /// <returns></returns>
        public override Task Stop()
        {
            return Task.CompletedTask;
        }
    }
}
