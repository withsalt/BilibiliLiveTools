using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Constants;
using Bilibili.AspNetCore.Apis.Exceptions;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models;
using Bilibili.AspNetCore.Apis.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CookieException = Bilibili.AspNetCore.Apis.Exceptions.CookieException;

namespace Bilibili.AspNetCore.Apis.Services
{
    public class BilibiliCookieService : IBilibiliCookieService
    {
        private readonly ILogger<BilibiliCookieService> _logger;
        private readonly IMemoryCache _cache;

        private static readonly object _locker = new object();

        private string _cookiePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cookies.json");

        public BilibiliCookieService(ILogger<BilibiliCookieService> logger
            , IMemoryCache cache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task SaveCookie(IEnumerable<CookieHeaderValue> cookies, string refreshToken)
        {
            if (cookies?.Any() != true) throw new ArgumentNullException(nameof(cookies), "Cookie data can not null");
            if (string.IsNullOrEmpty(refreshToken)) throw new ArgumentNullException(nameof(refreshToken), "Refresh token can not null.");
            await RemoveCookie();
            CookiesConfig cookiesConfig = new CookiesConfig()
            {
                Cookies = cookies,
                RefreshToken = refreshToken
            };
            if (cookiesConfig.IsExpired) throw new ArgumentException("Cookie has expired");
            string json = JsonUtil.SerializeObject(new CookiesJson(cookiesConfig), true);
            await File.WriteAllTextAsync(_cookiePath, json);
            _ = GetCookies(true);
        }

        public Task RemoveCookie()
        {
            lock (_locker)
            {
                _cache.Remove(CacheKeyConstant.COOKIE_KEY);
                if (File.Exists(_cookiePath))
                {
                    File.Delete(_cookiePath);
                }
                return Task.CompletedTask;
            }
        }

        public bool HasCookie()
        {
            try
            {
                CookiesConfig cookiesConfig = GetCookies();
                if (cookiesConfig != null && !cookiesConfig.IsExpired)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 是否即将过期
        /// </summary>
        /// <param name="minHours"></param>
        /// <returns></returns>
        public (bool, DateTimeOffset) WillExpired(int minHours = 24)
        {
            CookiesConfig cookiesConfig = GetCookies();
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

        public string GetString(bool force = false)
        {
            CookiesConfig cookiesConfig = GetCookies(force);
            if (cookiesConfig != null)
            {
                return cookiesConfig.GetCookieString();
            }
            return null;
        }

        public CookiesConfig GetCookies(bool force = false)
        {
            lock (_locker)
            {
                if (force)
                {
                    _cache.Remove(CacheKeyConstant.COOKIE_KEY);
                }
                CookiesConfig cookiesConfig = _cache.GetOrCreate(CacheKeyConstant.COOKIE_KEY, entry =>
                {
                    if (!File.Exists(_cookiePath))
                    {
                        throw new CookieException("File 'cookie.json' not fount.");
                    }
                    string result = File.ReadAllText(_cookiePath)?.Trim('\r', '\n', ' ');
                    if (string.IsNullOrWhiteSpace(result))
                    {
                        throw new CookieException("'Cookie内容为空，请先登录");
                    }
                    CookiesJson cookiesJson = JsonConvert.DeserializeObject<CookiesJson>(result);
                    var cookies = cookiesJson.ConvertTo();
                    if (cookies.IsExpired)
                    {
                        throw new CookieExpiredException("Cookie已过期");
                    }
                    return cookies;
                });
                return cookiesConfig;
            }
        }

        public string GetCsrf()
        {
            CookiesConfig cookies = GetCookies();
            if (cookies?.TryGetValue("bili_jct", out string jct) == true)
            {
                return jct;
            }
            throw new Exception("Get csrf from cookie failed.");
        }

        public string GetUserId()
        {
            CookiesConfig cookies = GetCookies();
            if (cookies?.TryGetValue("DedeUserID", out string jct) == true)
            {
                return jct;
            }
            throw new Exception("Get userid from cookie failed.");
        }

        public string GetRefreshToken()
        {
            CookiesConfig cookies = GetCookies();
            return cookies?.RefreshToken;
        }
    }
}
