using System;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models;
using BilibiliAutoLiver.Jobs.Scheduler;
using BilibiliAutoLiver.Models;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Services.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BilibiliAutoLiver.Services
{
    class StartupService : IStartupService
    {
        private readonly ILogger<StartupService> _logger;
        private readonly IBilibiliAccountApiService _accountService;
        private readonly IRefreshCookieJobSchedulerService _jobScheduler;
        private readonly LiveSettings _liveSetting;
        private readonly IPushStreamServiceV1 _pushServiceV1;
        private readonly IPushStreamServiceV2 _pushServiceV2;

        public StartupService(ILogger<StartupService> logger
            , IBilibiliAccountApiService accountService
            , IRefreshCookieJobSchedulerService jobScheduler
            , IOptions<LiveSettings> liveSettingOptions
            , IPushStreamServiceV1 pushServiceV1
            , IPushStreamServiceV2 pushServiceV2)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _jobScheduler = jobScheduler ?? throw new ArgumentNullException(nameof(jobScheduler));
            _liveSetting = liveSettingOptions.Value ?? throw new ArgumentNullException(nameof(liveSettingOptions));
            _pushServiceV1 = pushServiceV1 ?? throw new ArgumentNullException(nameof(pushServiceV1));
            _pushServiceV2 = pushServiceV2 ?? throw new ArgumentNullException(nameof(pushServiceV2));
        }

        public async Task Start()
        {
            var userInfo = await Login();
            //登录成功之后，启动定时任务
            await _jobScheduler.Start();
            //开始推流
            if (_liveSetting.V2?.IsEnabled== true)
            {
                if(_liveSetting.V1?.IsEnabled != true)
                {
                    throw new NotSupportedException("暂不支持使用V2的推流方式。");
                }
                //await StartPushV2();
                //return;
            }
            if (_liveSetting.V1?.IsEnabled == true)
            {
                await StartPushV1();
                return;
            }
            throw new NotSupportedException("V1和V2两种推流方式，至少启用一种！");
        }

        public async Task<UserInfo> Login()
        {
            var userInfo = await _accountService.LoginByCookie();
            if (userInfo == null)
            {
                userInfo = await _accountService.LoginByQrCode();
                if (userInfo == null)
                {
                    Environment.Exit(-1);
                }
            }
            _logger.LogInformation($"用户{userInfo.Uname}({userInfo.Mid})登录成功！");
            return userInfo;
        }

        public async Task StartPushV1()
        {
            try
            {
                await _pushServiceV1.CheckLiveSetting();
                await _pushServiceV1.CheckLiveRoom();
                await _pushServiceV1.CheckFFmpegBinary();
                await _pushServiceV1.Start();
            }
            catch (Exception ex)
            {
                _logger.LogError($"开启v1推流失败（{ex.Message}），应用程序退出。");
                Environment.Exit(-1);
            }
        }

        public async Task StartPushV2()
        {
            try
            {
                throw new NotSupportedException("目前暂不支持V2版本。");

                await _pushServiceV2.CheckLiveSetting();
                await _pushServiceV2.CheckLiveRoom();
                await _pushServiceV2.CheckFFmpegBinary();
                await _pushServiceV2.Start();
            }
            catch (Exception ex)
            {
                _logger.LogError($"开启v2推流失败（{ex.Message}），应用程序退出。");
                Environment.Exit(-1);
            }
        }
    }
}
