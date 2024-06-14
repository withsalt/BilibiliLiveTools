using System;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Repository.Interface;
using BilibiliAutoLiver.Services.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace BilibiliAutoLiver.Services.PushService
{
    public class PushStreamProxyService : IPushStreamProxyService
    {
        private readonly IAdvancePushStreamService _advancePush;
        private readonly INormalPushStreamService _normalPush;
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
            if (pushSetting == null || pushSetting.Model == ConfigModel.Easy)
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
