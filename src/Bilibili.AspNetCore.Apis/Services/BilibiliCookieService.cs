﻿using System;
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
        private readonly IHttpClientService _httpClient;
        private readonly IBilibiliCookieRepositoryProvider _cookieRepository;
        private readonly ILocalLockService _localLocker;

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

        private static string _key = "P*2D4R%aABTi^j3B";
        private static string _vector = "eRu!V2m4E&sQyAi#";

        public BilibiliCookieService(ILogger<BilibiliCookieService> logger
            , IMemoryCache cache
            , IBilibiliCookieRepositoryProvider cookieRepository
            , ILocalLockService localLocker)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _cookieRepository = cookieRepository ?? throw new ArgumentNullException(nameof(cookieRepository));
            _localLocker = localLocker ?? throw new ArgumentNullException(nameof(localLocker));
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
                        CookiesData cookies = new BilibiliCookieBuilder(_logger, _httpClient, cookieStr)
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

        public async Task<bool> RefreshCookie()
        {
            try
            {
                if (!await HasCookie())
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
                    csrf = await GetCsrf(),
                    refresh_csrf = refreshCsrf,
                    refresh_token = await GetRefreshToken()
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
                    csrf = await GetCsrf(),
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
