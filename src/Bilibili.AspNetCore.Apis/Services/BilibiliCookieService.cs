using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Constants;
using Bilibili.AspNetCore.Apis.Exceptions;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models;
using Bilibili.AspNetCore.Apis.Models.Base;
using Bilibili.AspNetCore.Apis.Models.Enums;
using Bilibili.AspNetCore.Apis.Providers;
using Bilibili.AspNetCore.Apis.Services.Cookie;
using Bilibili.AspNetCore.Apis.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Bilibili.AspNetCore.Apis.Services
{
    public class BilibiliCookieService : IBilibiliCookieService
    {
        private readonly ILogger<BilibiliCookieService> _logger;
        private readonly IMemoryCache _cache;
        private readonly IBilibiliCookieRepositoryProvider _cookieRepository;
        private readonly ILocalLockService _localLocker;
        private readonly IHttpClientFactory _httpClientFactory;

        private static string _key = "P*2D4R%aABTi^j3B";
        private static string _vector = "eRu!V2m4E&sQyAi#";

        public BilibiliCookieService(ILogger<BilibiliCookieService> logger
            , IMemoryCache cache
            , IBilibiliCookieRepositoryProvider cookieRepository
            , ILocalLockService localLocker
            , IHttpClientFactory httpClientFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _cookieRepository = cookieRepository ?? throw new ArgumentNullException(nameof(cookieRepository));
            _localLocker = localLocker ?? throw new ArgumentNullException(nameof(localLocker));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task SaveCookie(IEnumerable<CookieHeaderValue> cookies, string refreshToken)
        {
            if (cookies?.Any() != true) throw new ArgumentNullException(nameof(cookies), "Cookie data can not null");
            if (string.IsNullOrEmpty(refreshToken)) throw new ArgumentNullException(nameof(refreshToken), "Refresh token can not null.");
            await RemoveCookie();
            CookiesData cookiesConfig = new CookiesData()
            {
                Cookies = cookies,
                RefreshToken = refreshToken
            };
            if (cookiesConfig.IsExpired) throw new ArgumentException("Cookie has expired");
            string json = JsonUtil.SerializeObject(new CookiesJson(cookiesConfig), true);
            string data = AES.Encrypt(json, _key, _vector);
            await _cookieRepository.Write(data);
            _ = await GetCookies(true);
        }

        public async Task RemoveCookie()
        {
            await _cookieRepository.Delete();
            _cache.Remove(CacheKeyConstant.COOKIE_KEY);
        }

        public async Task<bool> HasCookie()
        {
            try
            {
                CookiesData cookiesConfig = await GetCookies();
                if (cookiesConfig != null && !cookiesConfig.IsExpired)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 是否即将过期
        /// </summary>
        /// <param name="minHours"></param>
        /// <returns></returns>
        public async Task<(bool, DateTimeOffset)> IsExpired(int minHours = 24)
        {
            CookiesData cookiesConfig = await GetCookies();
            var cookie = cookiesConfig?.Cookies?.FirstOrDefault(p => p.Cookies.Any(q => q.Name == "bili_jct") && p.Expires >= DateTime.UtcNow);
            if (cookie == null)
            {
                throw new Exception("无效Cookie，没有bili_jct条目");
            }
            var ts = cookie.Expires - DateTime.UtcNow;
            if (!ts.HasValue)
            {
                throw new Exception("无效Cookie，没有过期时间");
            }
            if (ts.Value.TotalHours >= minHours)
            {
                return (false, cookie.Expires.Value.ToLocalTime());
            }
            return (true, cookie.Expires.Value.ToLocalTime());
        }

        public async Task<string> GetString(bool force = false)
        {
            CookiesData cookiesConfig = await GetCookies(force);
            if (cookiesConfig != null)
            {
                return cookiesConfig.GetCookieString();
            }
            return null;
        }

        public async Task<CookiesData> GetCookies(bool force = false)
        {
            if (force)
            {
                _cache.Remove(CacheKeyConstant.COOKIE_KEY);
            }
            string lockKey = "GET_COOKIES_SPIN_LOCK_KEY";
            bool isLocked = false;
            try
            {
                isLocked = _localLocker.SpinLock(lockKey, 60);
                if (!isLocked)
                {
                    _logger.LogWarning("获取Cookie时加锁失败");
                    return null;
                }
                CookiesData cookiesConfig = await _cache.GetOrCreateAsync(CacheKeyConstant.COOKIE_KEY, async entry =>
                {
                    try
                    {
                        string cookieStr = await _cookieRepository.Read().ConfigureAwait(false);
                        if (!AES.TryDecrypt(cookieStr, _key, _vector, out cookieStr))
                        {
                            return null;
                        }
                        //构建Cookie（注意：有先后顺序）
                        CookiesData cookies = new BilibiliCookieBuilder(_logger, _httpClientFactory, cookieStr)
                            .SetBnut()
                            .SetUuid()
                            .SetLsid()
                            .SetBuvid3_4()
                            .SetBuvidFp()
                            .SetTicket()
                            .Build();
                        //设置当前缓存过期时间和ticket过期时间一致，如果ticket为空，那么就是10分钟
                        entry.AbsoluteExpirationRelativeToNow = cookies.HasTicket ? (cookies.TicketExpireIn - DateTime.UtcNow.AddMinutes(60)) : TimeSpan.FromMinutes(10);
                        return cookies;
                    }
                    catch (CookieException ex)
                    {
                        _logger.LogWarning($"获取Cookie失败，{ex.Message}");
                        return null;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"获取Cookie失败，{ex.Message}");
                        return null;
                    }
                });
                return cookiesConfig;
            }
            finally
            {
                if (isLocked)
                    _localLocker.SpinUnLock(lockKey);
            }
        }

        public async Task<string> GetCsrf()
        {
            CookiesData cookies = await GetCookies();
            if (cookies?.TryGetValue("bili_jct", out string jct) == true)
            {
                return jct;
            }
            throw new Exception("Get csrf from cookie failed.");
        }

        public async Task<string> GetUserId()
        {
            CookiesData cookies = await GetCookies();
            if (cookies?.TryGetValue("DedeUserID", out string jct) == true)
            {
                return jct;
            }
            throw new Exception("Get userid from cookie failed.");
        }

        public async Task<string> GetRefreshToken()
        {
            CookiesData cookies = await GetCookies();
            return cookies?.RefreshToken;
        }

        #region privite



        #endregion

    }
}
