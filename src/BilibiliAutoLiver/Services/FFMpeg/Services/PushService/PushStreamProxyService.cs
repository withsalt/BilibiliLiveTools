using System;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Repository.Interface;
using BilibiliAutoLiver.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Services.FFMpeg.Services.PushService
{
    public class PushStreamProxyService : IPushStreamProxyService
    {
        private readonly ILogger<PushStreamProxyService> _logger;
        private readonly IAdvancePushStreamService _advancePush;
        private readonly INormalPushStreamService _normalPush;
        private readonly IServiceProvider _serviceProvider;

        private ConfigModel _pushModel;
        private static readonly object _lock = new object();

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

        private PushSetting GetPushSetting()
        {
            PushSetting pushSetting = null;
            using (var scope = _serviceProvider.CreateScope())
            {
                pushSetting = scope.ServiceProvider.GetRequiredService<IPushSettingRepository>().Where(p => !p.IsDeleted).First();
            }
            return pushSetting;
        }

        private IPushStreamService GetService()
        {
            if (_pushModel == ConfigModel.None)
            {
                lock (_lock)
                {
                    PushSetting pushSetting = GetPushSetting();
                    if (pushSetting != null)
                    {
                        _pushModel = pushSetting.Model;
                    }
                }
            }
            if (_pushModel == ConfigModel.Normal)
                return _normalPush;
            else if (_pushModel == ConfigModel.Advance)
                return _advancePush;
            throw new NotSupportedException($"未知的推流方式：{_pushModel}");
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
            var service = GetService();
            await service.CheckLiveSetting();
            await service.CheckLiveRoom();
            await service.CheckFFmpegBinary();
            return await service.Start();
        }

        public async Task<bool> Stop()
        {
            bool isStop = await GetService().Stop();
            if (isStop)
            {
                //停止的时候，清空模式
                //这样只有重启推流的时候，才会更新推流模式，因为更新配置文件之后，不一定会重新推流
                lock (_lock)
                {
                    _pushModel = ConfigModel.None;
                }
            }
            return isStop;
        }

        public PushStatus GetStatus()
        {
            if (_pushModel == ConfigModel.None)
            {
                return PushStatus.Stopped;
            }
            return GetService().GetStatus();
        }

        public void Dispose()
        {
            _advancePush.Dispose();
            _normalPush.Dispose();
        }
    }
}
