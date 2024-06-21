using System;
using System.Threading;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models;
using BilibiliAutoLiver.Config;
using BilibiliAutoLiver.Jobs.Scheduler;
using BilibiliAutoLiver.Services.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Services
{
    class StartupService : IStartupService
    {
        private readonly ILogger<StartupService> _logger;
        private readonly IBilibiliAccountApiService _accountService;
        private readonly IJobSchedulerService _jobScheduler;
        private readonly IPushStreamProxyService _pushProxyService;
        private readonly IMemoryCache _cache;

        public StartupService(ILogger<StartupService> logger
            , IBilibiliAccountApiService accountService
            , IJobSchedulerService jobScheduler
            , IPushStreamProxyService pushProxyService
            , IMemoryCache cache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _jobScheduler = jobScheduler ?? throw new ArgumentNullException(nameof(jobScheduler));
            _pushProxyService = pushProxyService ?? throw new ArgumentNullException(nameof(pushProxyService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task Start(CancellationToken token)
        {
            try
            {
                var userInfo = await Login();
                if (userInfo == null)
                {
                    _logger.LogWarning("用户未登录！");
                    return;
                }
                //登录成功之后，启动定时任务
                await _jobScheduler.Start(token);
                //开始推流
                await _pushProxyService.Start();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化失败！");
            }
        }

        public async Task<UserInfo> Login()
        {
            try
            {
                _cache.Set(CacheKeyConstant.LOGING_STATUS_CACHE_KEY, true, TimeSpan.FromMinutes(10));
                //通过保存的Cookie登录
                UserInfo userInfo = await _accountService.LoginByCookie();
                _cache.Remove(CacheKeyConstant.LOGING_STATUS_CACHE_KEY);
                if (userInfo == null)
                {
                    //通过扫描二维码登录
                    userInfo = await _accountService.LoginByQrCode();
                }
                if (userInfo == null)
                {
                    //通过Cookie和二维码登录都未成功，那么挂起，直到完成用户登录
                    while (!_accountService.IsLogged())
                    {
                        await Task.Delay(1000);
                    }
                    userInfo = await _accountService.LoginByCookie();
                }
                if (userInfo != null)
                {
                    _logger.LogInformation($"用户{userInfo.Uname}({userInfo.Mid})登录成功！");
                }
                return userInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"用户登录失败");
                return null;
            }
            finally
            {
                _cache.Remove(CacheKeyConstant.LOGING_STATUS_CACHE_KEY);
            }
        }
    }
}
