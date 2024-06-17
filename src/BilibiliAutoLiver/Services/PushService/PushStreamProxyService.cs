using System;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Repository.Interface;
using BilibiliAutoLiver.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Services.PushService
{
    public class PushStreamProxyService : IPushStreamProxyService
    {
        private readonly ILogger<PushStreamProxyService> _logger;
        private readonly IAdvancePushStreamService _advancePush;
        private readonly INormalPushStreamService _normalPush;
        private readonly IServiceProvider _serviceProvider;

        public PushStreamProxyService(ILogger<PushStreamProxyService> logger
            , IServiceProvider serviceProvider
            , IAdvancePushStreamService advancePush
            , INormalPushStreamService normalPush)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _advancePush = advancePush ?? throw new ArgumentNullException(nameof(advancePush));
            _normalPush = normalPush ?? throw new ArgumentNullException(nameof(normalPush));
        }

        private IPushStreamService GetService()
        {
            PushSetting pushSetting = null;
            using (var scope = _serviceProvider.CreateScope())
            {
                pushSetting = scope.ServiceProvider.GetRequiredService<IPushSettingRepository>().Where(p => !p.IsDeleted).First();
            }
            if (pushSetting == null || pushSetting.Model == ConfigModel.Normal)
                return _normalPush;
            else if (pushSetting.Model == ConfigModel.Advance)
                return _advancePush;
            throw new NotSupportedException($"未知的推流方式：{pushSetting.Model}");
        }

        public async Task CheckFFmpegBinary()
        {
            await GetService().CheckFFmpegBinary();
        }

        public async Task CheckLiveRoom()
        {
            await GetService().CheckLiveRoom();
        }

        public async Task CheckLiveSetting()
        {
            await GetService().CheckLiveSetting();
        }

        public async Task<bool> Start()
        {
            try
            {
                var service = GetService();
                await service.CheckLiveSetting();
                await service.CheckLiveRoom();
                await service.CheckFFmpegBinary();
                return await service.Start();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"开启推流失败，{ex.Message}");
                return false;
            }
        }

        public async Task<bool> Stop()
        {
            return await GetService().Stop();
        }

        public PushStatus GetStatus()
        {
            return GetService().GetStatus();
        }

        public void Dispose()
        {
            GetService().Dispose();
        }
    }
}
