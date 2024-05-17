using System;
using System.Threading;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models;
using BilibiliAutoLiver.Config;
using BilibiliAutoLiver.Jobs.Scheduler;
using BilibiliAutoLiver.Models.Settings;
using BilibiliAutoLiver.Services.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BilibiliAutoLiver.Services
{
    class StartupService : IStartupService
    {
        private readonly ILogger<StartupService> _logger;
        private readonly IMemoryCache _cache;
        private readonly IBilibiliAccountApiService _accountService;
        private readonly IJobSchedulerService _jobScheduler;
        private readonly IPushStreamProxyService _pushProxyService;

        public StartupService(ILogger<StartupService> logger
            , IMemoryCache cache
            , IBilibiliAccountApiService accountService
            , IJobSchedulerService jobScheduler
            , IPushStreamProxyService pushProxyService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _jobScheduler = jobScheduler ?? throw new ArgumentNullException(nameof(jobScheduler));
            _pushProxyService = pushProxyService ?? throw new ArgumentNullException(nameof(pushProxyService));
        }

        public async Task Start(CancellationToken token)
        {
            try
            {
                var userInfo = await Login();
                //登录成功之后，启动定时任务
                await _jobScheduler.StartAsync(token);

#if DEBUG
                _logger.LogInformation("Debug模式，不开启推流");
#endif

                //开始推流
                await _pushProxyService.CheckLiveSetting();
                await _pushProxyService.CheckLiveRoom();
                await _pushProxyService.CheckFFmpegBinary();
                await _pushProxyService.Start();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化失败！");
                Environment.Exit(-1);
            }
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
            _cache.Set<bool>(CacheKeyConstant.LOGIN_STATUS, true);
            _logger.LogInformation($"用户{userInfo.Uname}({userInfo.Mid})登录成功！");
            return userInfo;
        }
    }
}
