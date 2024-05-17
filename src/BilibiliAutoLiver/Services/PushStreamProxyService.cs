using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models;
using BilibiliAutoLiver.Models.Settings;
using BilibiliAutoLiver.Plugin.Base;
using BilibiliAutoLiver.Services.Base;
using BilibiliAutoLiver.Services.Interface;
using BilibiliAutoLiver.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BilibiliAutoLiver.Services
{
    public class PushStreamProxyService : IPushStreamProxyService
    {
        private readonly IPushStreamServiceV1 _v1;
        private readonly IPushStreamServiceV2 _v2;
        private readonly LiveSettings _liveSetting;

        public PushStreamProxyService(IOptions<LiveSettings> liveSettingOptions
            , IPushStreamServiceV1 v1
            , IPushStreamServiceV2 v2)
        {
            _liveSetting = liveSettingOptions.Value ?? throw new ArgumentNullException(nameof(liveSettingOptions));
            _v1 = v1 ?? throw new ArgumentNullException(nameof(v1));
            _v2 = v2 ?? throw new ArgumentNullException(nameof(v2));
        }

        private IPushStreamService GetPushStreamService()
        {
            if (_liveSetting.V2?.IsEnabled == true)
                return _v2;
            else if (_liveSetting.V1?.IsEnabled == true)
                return _v1;
            throw new NotSupportedException("V1和V2两种推流方式，至少启用一种！");
        }

        public async Task CheckFFmpegBinary()
        {
            await GetPushStreamService().CheckFFmpegBinary();
        }

        public async Task CheckLiveRoom()
        {
            await GetPushStreamService().CheckLiveRoom();
        }

        public async Task CheckLiveSetting()
        {
            await GetPushStreamService().CheckLiveSetting();
        }

        public async Task Start()
        {
            await GetPushStreamService().Start();
        }

        public async Task Stop()
        {
            await GetPushStreamService().Stop();
        }

        public void Dispose()
        {
            GetPushStreamService().Dispose();
        }
    }
}
