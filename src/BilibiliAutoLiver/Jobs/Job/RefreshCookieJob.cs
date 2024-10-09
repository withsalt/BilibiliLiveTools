using System;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using BilibiliAutoLiver.Config;
using BilibiliAutoLiver.Jobs.Interface;
using BilibiliAutoLiver.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BilibiliAutoLiver.Jobs.Job
{
    [DisallowConcurrentExecution]
    public class RefreshCookieJob : BaseJobDescribe, IJob
    {
        private readonly ILogger<RefreshCookieJob> _logger;
        private readonly IMemoryCache _cache;
        private readonly IBilibiliAccountApiService _accountService;
        private readonly IBilibiliCookieService _cookie;

        public RefreshCookieJob(ILogger<RefreshCookieJob> logger
            , IMemoryCache cache
            , IBilibiliAccountApiService accountService
            , IBilibiliCookieService cookie)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _cookie = cookie ?? throw new ArgumentNullException(nameof(cookie));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await RefreshCookie();
        }

        private async Task RefreshCookie()
        {
            if (_cache.TryGetValue(CacheKeyConstant.LAST_REFRESH_COOKIE_TIME_CACHE_KEY, out DateTime lastRefreshTime) && lastRefreshTime != DateTime.MinValue)
            {
                if ((DateTime.UtcNow - lastRefreshTime).TotalHours < 6)
                {
                    //每6个小时检查一次是否需要刷新
                    return;
                }
            }
            bool updateStatus = false;
            try
            {
                _logger.LogInformation("定时刷新Cookie开始。");
                //刷新cookie
                if (!await _cookie.HasCookie())
                {
                    _logger.LogInformation("定时刷新Cookie失败，未登录。");
                    return;
                }
                if (await _cookie.CookieNeedToRefresh())
                {
                    _logger.LogInformation("检测到Cookie需要刷新，刷新Cookie。");
                }
                else
                {
                    var cookieWillExpired = await _cookie.WillExpired();
                    if (!cookieWillExpired.Item1)
                    {
                        updateStatus = true;
                        _logger.LogInformation($"定时刷新Cookie完成，无需刷新。Cookie过期时间：{cookieWillExpired.Item2.ToString("yyyy-MM-dd HH:mm:ss")}。");
                        return;
                    }
                }
                updateStatus = await _cookie.RefreshCookie();
                if (!updateStatus)
                {
                    _logger.LogWarning("定时刷新Cookie失败，具体信息请查看日志。");
                    return;
                }
                var userInfo1 = await _accountService.GetUserInfo(false);
                if (userInfo1 == null)
                {
                    _logger.LogInformation("定时刷新Cookie失败，新Cookie获取员工失败。");
                    return;
                }
                _logger.LogInformation($"用户{userInfo1.Uname}定时刷新Cookie完成。当前用户登录状态：{userInfo1.IsLogin}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"定时刷新Cookie失败，{ex.Message}");
            }
            finally
            {
                if (updateStatus)
                    _cache.Set(CacheKeyConstant.LAST_REFRESH_COOKIE_TIME_CACHE_KEY, DateTime.UtcNow);
            }
        }

        #region Job info

        private JobMetadata _jobMetadata = null;

        protected override JobMetadata JobMetadata
        {
            get
            {
                if (_jobMetadata != null)
                {
                    return _jobMetadata;
                }
                _jobMetadata = new JobMetadata()
                {
                    Id = 1,
                    Name = $"{this.GetType().Name}Service",
                    IntervalTime = 600,  //10分钟
                    StartTime = DateTime.Now.AddSeconds(60)
                };
                return _jobMetadata;
            }
        }

        #endregion

    }
}
