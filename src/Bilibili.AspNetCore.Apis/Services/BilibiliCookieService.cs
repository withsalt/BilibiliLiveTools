using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly IHttpClientService _httpClient;

        private static readonly object _locker = new object();

        private string _cookiePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cookies.json");

        /// <summary>
        /// 获取cookie是否需要刷新
        /// </summary>
        private const string _cookieInfo = "https://passport.bilibili.com/x/passport-login/web/cookie/info";

        /// <summary>
        /// 获取RefreshCsrf
        /// </summary>
        private const string _getRefreshCsrf = "https://www.bilibili.com/correspond/1/{0}";

        /// <summary>
        /// 刷新cookie
        /// </summary>
        private const string _refreshCookie = "https://passport.bilibili.com/x/passport-login/web/cookie/refresh";

        /// <summary>
        /// 确认刷新
        /// </summary>
        private const string _confirmRefresh = "https://passport.bilibili.com/x/passport-login/web/confirm/refresh";

        public BilibiliCookieService(ILogger<BilibiliCookieService> logger
            , IMemoryCache cache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _httpClient = new HttpClientService(this);
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
                CookiesData cookiesConfig = GetCookies();
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
            CookiesData cookiesConfig = GetCookies();
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
            CookiesData cookiesConfig = GetCookies(force);
            if (cookiesConfig != null)
            {
                return cookiesConfig.GetCookieString();
            }
            return null;
        }

        public CookiesData GetCookies(bool force = false)
        {
            lock (_locker)
            {
                if (force)
                {
                    _cache.Remove(CacheKeyConstant.COOKIE_KEY);
                }
                CookiesData cookiesConfig = _cache.GetOrCreate(CacheKeyConstant.COOKIE_KEY, entry =>
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
                    //构建Cookie（注意：有先后顺序）
                    CookiesData cookies = new BilibiliCookieBuilder(_logger, _httpClient, result)
                        .SetBnut().SetUuid().SetLsid()
                        .SetBuvid3_4().SetBuvidFp().SetTicket()
                        .Build();
                    //设置当前缓存过期时间和ticket过期时间一致，如果ticket为空，那么就是10分钟
                    entry.AbsoluteExpirationRelativeToNow = cookies.HasTicket ? (cookies.TicketExpireIn - DateTime.UtcNow) : TimeSpan.FromMinutes(10);
                    return cookies;
                });
                return cookiesConfig;
            }
        }

        public string GetCsrf()
        {
            CookiesData cookies = GetCookies();
            if (cookies?.TryGetValue("bili_jct", out string jct) == true)
            {
                return jct;
            }
            throw new Exception("Get csrf from cookie failed.");
        }

        public string GetUserId()
        {
            CookiesData cookies = GetCookies();
            if (cookies?.TryGetValue("DedeUserID", out string jct) == true)
            {
                return jct;
            }
            throw new Exception("Get userid from cookie failed.");
        }

        public string GetRefreshToken()
        {
            CookiesData cookies = GetCookies();
            return cookies?.RefreshToken;
        }

        public async Task<bool> RefreshCookie()
        {
            try
            {
                if (!HasCookie())
                {
                    throw new Exception("未登录，请先登录！");
                }
                string key = GetCorrespondPath();
                var refreshCsrfRt = await _httpClient.Execute<string>(string.Format(_getRefreshCsrf, key), HttpMethod.Get, getRowData: true);
                if (string.IsNullOrWhiteSpace(refreshCsrfRt.Data))
                {
                    throw new Exception($"获取refresh csrf失败！");
                }

                string pattern = @"<div id=""1-name"">(?<content>.*?)</div>";
                Match match = Regex.Match(refreshCsrfRt.Data, pattern);
                if (!match.Success)
                {
                    throw new Exception($"获取refresh csrf失败，返回内容：{refreshCsrfRt.Data}");
                }
                string refreshCsrf = match.Groups["content"]?.Value;
                if (string.IsNullOrWhiteSpace(refreshCsrf))
                {
                    throw new Exception($"获取refresh csrf失败");
                }
                //刷新cookie
                RefreshCookieModel refreshCookieModel = new RefreshCookieModel()
                {
                    csrf = GetCsrf(),
                    refresh_csrf = refreshCsrf,
                    refresh_token = GetRefreshToken()
                };
                ResultModel<RefreshCookieResult> refreshCookieResult = await _httpClient.Execute<RefreshCookieResult>(_refreshCookie, HttpMethod.Post, refreshCookieModel, BodyFormat.Form_UrlEncoded);
                if (refreshCookieResult == null)
                {
                    throw new Exception($"刷新cookie失败。{refreshCookieResult?.Message}");
                }
                if (refreshCookieResult.Code != 0)
                {
                    throw new Exception($"刷新cookie失败。{refreshCookieResult?.Message}");
                }
                if (string.IsNullOrWhiteSpace(refreshCookieResult?.Data.refresh_token) || refreshCookieResult.Cookies?.Any() != true)
                {
                    throw new Exception($"刷新cookie失败。返回数据为空");
                }
                await SaveCookie(refreshCookieResult.Cookies, refreshCookieResult.Data.refresh_token);
                //确认刷新cookie
                ConfirmRefreshModel confirmRefreshModel = new ConfirmRefreshModel()
                {
                    csrf = GetCsrf(),
                    refresh_token = refreshCookieModel.refresh_token,
                };
                ResultModel<object> confirmRefreshResult = await _httpClient.Execute<object>(_confirmRefresh, HttpMethod.Post, confirmRefreshModel, BodyFormat.Form_UrlEncoded);
                if (confirmRefreshResult == null || confirmRefreshResult.Code != 0)
                {
                    throw new Exception($"刷新cookie失，确认更新失败。{confirmRefreshResult?.Message}");
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刷新Cookie失败，404可能是接口请求频次限制");
                return false;
            }
        }

        public async Task<bool> CookieNeedToRefresh()
        {
            try
            {
                var result = await _httpClient.Execute<CookieInfo>(_cookieInfo, HttpMethod.Get);
                if (result == null || result.Code != 0 || result.Data == null)
                {
                    throw new Exception($"获取Cookie信息失败！{result?.Message}");
                }
                return result.Data.refresh;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"校验Cookie是否有效失败，{ex.Message}");
                return false;
            }
        }

        #region privite

        private string GetCorrespondPath()
        {
            var publicKeyPEM = @"
            -----BEGIN PUBLIC KEY-----
            MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDLgd2OAkcGVtoE3ThUREbio0Eg
            Uc/prcajMKXvkCKFCWhJYJcLkcM2DKKcSeFpD/j6Boy538YXnR6VhcuUJOhH2x71
            nzPjfdTcqMz7djHum0qSZA0AyCBDABUqCrfNgCiJ00Ra7GmRj+YCK1NJEuewlb40
            JNrRuoEUXpabUzGB8QIDAQAB
            -----END PUBLIC KEY-----";

            //感觉B站服务器时间有问题？这里默认给减去1分钟
            string ts = DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeMilliseconds().ToString();
            var oaepsha256 = RSAEncryptionPadding.OaepSHA256;
            using var rsa = RSA.Create();
            rsa.ImportFromPem(publicKeyPEM);
            var encryptedData = rsa.Encrypt(Encoding.UTF8.GetBytes($"refresh_{ts}"), oaepsha256);
            var sb = new StringBuilder();
            foreach (var b in encryptedData)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }

        #endregion

    }
}
