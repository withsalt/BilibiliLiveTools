using System;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models.Settings;
using BilibiliAutoLiver.Repository.Interface;
using BilibiliAutoLiver.Services.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Models.Enums;

namespace BilibiliAutoLiver.Services
{
    public class PushStreamProxyService : IPushStreamProxyService
    {
        private readonly IAdvancePushStreamService _advancePush;
        private readonly INormalPushStreamService _normalPush;
        private readonly LiveSettings _liveSetting;
        private readonly IServiceProvider _serviceProvider;

        public PushStreamProxyService(IServiceProvider serviceProvider
            , IAdvancePushStreamService advancePush
            , INormalPushStreamService normalPush)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _advancePush = advancePush ?? throw new ArgumentNullException(nameof(advancePush));
            _normalPush = normalPush ?? throw new ArgumentNullException(nameof(normalPush));
        }

        private IPushStreamService GetPushStreamService()
        {
            PushSetting pushSetting = null;
            using (var scope = _serviceProvider.CreateScope())
            {
                pushSetting = scope.ServiceProvider.GetRequiredService<IPushSettingRepository>().Where(p => !p.IsDeleted).First();
            }
            if (pushSetting == null)
            {
                throw new ArgumentNullException("未配置推流设置");
            }

            if (pushSetting.Model == ConfigModel.Easy)
                return _normalPush;
            else if (pushSetting.Model == ConfigModel.Advance)
                return _advancePush;
            throw new NotSupportedException($"未知的推流方式：{pushSetting.Model}");
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
