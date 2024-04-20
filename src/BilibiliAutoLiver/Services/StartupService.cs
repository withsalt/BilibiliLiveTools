using System;
using System.Threading.Tasks;
using BilibiliAutoLiver.Jobs.Scheduler;
using BilibiliLiveCommon.Model;
using BilibiliLiveCommon.Services.Interface;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Services
{
    class StartupService : IStartupService
    {
        private readonly ILogger<StartupService> _logger;
        private readonly IBilibiliAccountService _accountService;
        private readonly IRefreshCookieJobSchedulerService _jobScheduler;
        private readonly IPushStreamServiceV1 _pushServiceV1;

        public StartupService(ILogger<StartupService> logger
            , IBilibiliAccountService accountService
            , IRefreshCookieJobSchedulerService jobScheduler
            , IPushStreamServiceV1 pushServiceV1)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _jobScheduler = jobScheduler ?? throw new ArgumentNullException(nameof(jobScheduler));
            _pushServiceV1 = pushServiceV1 ?? throw new ArgumentNullException(nameof(pushServiceV1));
        }

        public async Task Start()
        {
            var userInfo = await Login();
            //登录成功之后，启动定时任务
            await _jobScheduler.Start();
            //开始推流
            await StartPushV1();
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

                //开始推流
                await _pushServiceV1.StartPush();
            }
            catch (Exception ex)
            {
                Environment.Exit(-1);
            }
        }
    }
}
