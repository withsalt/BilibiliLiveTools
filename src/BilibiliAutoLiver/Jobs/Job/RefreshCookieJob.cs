using System;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models;
using BilibiliAutoLiver.Config;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BilibiliAutoLiver.Jobs.Job
{
    [DisallowConcurrentExecution]
    public class RefreshCookieJob : IJob
    {
        private readonly ILogger<RefreshCookieJob> _logger;
        private readonly IMemoryCache _cache;
        private readonly IBilibiliAccountApiService _accountService;
        private readonly IBilibiliCookieService _cookieService;

        public RefreshCookieJob(ILogger<RefreshCookieJob> logger
            , IMemoryCache cache
            , IBilibiliAccountApiService accountService
            , IBilibiliCookieService cookieService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _cookieService = cookieService ?? throw new ArgumentNullException(nameof(cookieService));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await RefreshCookie();

            await SendHeartBeat();
        }

        public async Task SendHeartBeat()
        {
            try
            {
                _logger.LogInformation("发送心跳请求");
                //心跳
                await _accountService.HeartBeat();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"心跳定时任务执行失败，{ex.Message}");
            }
        }

        private async Task RefreshCookie()
        {
            if (_cache.TryGetValue(CacheKeyConstant.LAST_REFRESH_COOKIE_TIME, out DateTime lastRefreshTime) && lastRefreshTime != DateTime.MinValue)
            {
                if ((DateTime.UtcNow - lastRefreshTime).TotalHours < 6)
                {
                    //每6个小时检查一次是否需要刷新
                    return;
                }
            }
            else
            {
                _cache.Set(CacheKeyConstant.LAST_REFRESH_COOKIE_TIME, DateTime.UtcNow);
                return;
            }

            try
            {
                //刷新cookie
                if (!_cookieService.HasCookie())
                {
                    _logger.LogInformation("定时刷新Cookie失败，未登录。");
                    return;
                }
                if (await _accountService.CookieNeedToRefresh())
                {
                    _logger.LogInformation("检测到Cookie需要刷新，刷新Cookie。");
                }
                else
                {
                    UserInfo userInfo0 = await _accountService.GetUserInfo();
                    if (userInfo0 == null)
                    {
                        _logger.LogInformation("定时刷新Cookie失败，获取用户信息失败，无效Cookie。");
                        return;
                    }
                    var cookieWillExpired = _cookieService.WillExpired();
                    if (!cookieWillExpired.Item1)
                    {
                        _logger.LogInformation($"定时刷新Cookie完成，Cookie过期时间：{cookieWillExpired.Item2.ToString("yyyy-MM-dd HH:mm:ss")}，无需刷新。");
                        return;
                    }
                }
                await _accountService.RefreshCookie();
                var userInfo1 = await _accountService.GetUserInfo();
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
                _cache.Set(CacheKeyConstant.LAST_REFRESH_COOKIE_TIME, DateTime.UtcNow);
            }
        }
    }
}
