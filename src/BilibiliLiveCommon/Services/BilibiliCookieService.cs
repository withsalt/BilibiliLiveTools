using BilibiliLiveCommon.Config;
using BilibiliLiveCommon.Exceptions;
using BilibiliLiveCommon.Services.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace BilibiliLiveCommon.Services
{
    public class BilibiliCookieService : IBilibiliCookieService
    {
        private readonly ILogger<BilibiliCookieService> _logger;
        private readonly IMemoryCache _cache;

        private string _cookiePath = Path.Combine(Environment.CurrentDirectory, "config.json");

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
            CookiesConfig cookiesConfig = new CookiesConfig()
            {
                Cookies = cookies,
                RefreshToken = refreshToken
            };
            if (cookiesConfig.IsExpired) throw new ArgumentException("Cookie has expired");
            string json = JsonConvert.SerializeObject(new CookiesJson(cookiesConfig));
            await File.WriteAllTextAsync(_cookiePath, json);
            _ = GetCookies(true);
        }

        public bool HasCookie()
        {
            CookiesConfig cookiesConfig = GetCookies();
            if (cookiesConfig != null && !cookiesConfig.IsExpired)
            {
                return true;
            }
            return false;
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
