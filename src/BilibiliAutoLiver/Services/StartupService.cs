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

        public StartupService(ILogger<StartupService> logger
            , IBilibiliAccountService accountService
            , IRefreshCookieJobSchedulerService jobScheduler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _jobScheduler = jobScheduler ?? throw new ArgumentNullException(nameof(jobScheduler));
        }

        public async Task Start()
        {
            var userInfo = await Login();
            //登录成功之后，启动定时任务
            await _jobScheduler.Start();
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
    }
}
