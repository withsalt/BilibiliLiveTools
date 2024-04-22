using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Constants;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models;
using Bilibili.AspNetCore.Apis.Models.Base;
using Bilibili.AspNetCore.Apis.Models.Enums;
using Bilibili.AspNetCore.Apis.Utils;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Bilibili.AspNetCore.Apis.Services
{
    public class BilibiliAccountService : IBilibiliAccountService
    {
        private string _navApi = "https://api.bilibili.com/x/web-interface/nav";

        private const string _heartBeatApi = "https://api.live.bilibili.com/relation/v1/Feed/heartBeat";

        /// <summary>
        /// 生成二维码
        /// </summary>
        private const string _generateQrCodeApi = "https://passport.bilibili.com/x/passport-login/web/qrcode/generate";

        /// <summary>
        /// 二维码是否扫描
        /// </summary>
        private const string _qrCodeHasScanedApi = "https://passport.bilibili.com/x/passport-login/web/qrcode/poll?qrcode_key={0}&source=main_mini";

        /// <summary>
        /// 获取cookie是否需要刷新
        /// </summary>
        private const string _cookieInfo = "https://passport.bilibili.com/x/passport-login/web/cookie/info";

        private const string _getRefreshCsrf = "https://www.bilibili.com/correspond/1/{0}";

        /// <summary>
        /// 刷新cookie
        /// </summary>
        private const string _refreshCookie = "https://passport.bilibili.com/x/passport-login/web/cookie/refresh";

        /// <summary>
        /// 确认刷新
        /// </summary>
        private const string _confirmRefresh = "https://passport.bilibili.com/x/passport-login/web/confirm/refresh";

        private readonly IHttpClientService _httpClient;
        private readonly IBilibiliCookieService _cookieService;
        private readonly ILogger<BilibiliAccountService> _logger;
        private readonly IServer _server;
        private readonly IMemoryCache _cache;

        public BilibiliAccountService(ILogger<BilibiliAccountService> logger
            , IHttpClientService httpClient
            , IBilibiliCookieService cookieService
            , IServer server
            , IMemoryCache cache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _cookieService = cookieService ?? throw new ArgumentNullException(nameof(cookieService));
            _server = server ?? throw new ArgumentNullException(nameof(IServer));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<UserInfo> GetUserInfo()
        {
            try
            {
                var result = await _httpClient.Execute<UserInfo>(_navApi, HttpMethod.Get);
                if (result == null || result.Data == null)
                {
                    throw new Exception("通过Cookie登录失败，返回结果为空！");
                }
                if (!result.Data.IsLogin)
                {
                    throw new Exception("通过Cookie登录失败，可能Cookie已经失效，请重新获取Cookie");
                }
                return result.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        public async Task<UserInfo> LoginByCookie()
        {
            if (!_cookieService.HasCookie())
            {
                return null;
            }
            if (_cookieService.WillExpired().Item1)
            {
                _logger.LogInformation($"Cookie即将过期，过期时间：{_cookieService.WillExpired().Item2}，刷新Cookie。");
                await RefreshCookie();
            }
            if (await CookieNeedToRefresh())
            {
                _logger.LogInformation("检测到Cookie需要刷新，刷新Cookie。");
                await RefreshCookie();
            }
            return await GetUserInfo();
        }

        public async Task<UserInfo> LoginByQrCode()
        {
            UserInfo userInfo = null;
            try
            {
                await _cookieService.RemoveCookie();
                _logger.LogWarning("开始使用扫描二维码登录！");
                List<string> endpoints = GetEndpoint();
                if (endpoints?.Any() != true)
                {
                    throw new Exception("无法获取本地IP地址信息。");
                }
                _logger.LogWarning($"请在10分钟之内，在任意浏览器打开以下任意一个链接进行扫描登录：{JsonUtil.SerializeObject(endpoints)}");

                int timeout = 10 * 60 * 1000;
                int expTime = 180;
                int genIndex = 1;
                bool hasCookie = false;

                Stopwatch stopwatch = Stopwatch.StartNew();
                while (!hasCookie && stopwatch.ElapsedMilliseconds < timeout)
                {
                    QrCodeUrl qrCodeInfo = await GenerateQrCode();
                    _logger.LogInformation($"第{genIndex}次生成登录二维码。");

                    QrCodeLoginStatus loginStatus = new QrCodeLoginStatus()
                    {
                        Index = genIndex,
                        IsLogged = false,
                        QrCodeEffectiveTime = 180,
                        QrCode = "data:image/png;base64," + Convert.ToBase64String(qrCodeInfo.GetBytes())
                    };
                    _cache.Set(CacheKeyConstant.LOGIN_STATUS_CACHE_KEY, loginStatus);

                    Stopwatch qrCodeExp = Stopwatch.StartNew();
                    while (qrCodeExp.ElapsedMilliseconds < (expTime - 10) * 1000)
                    {
                        int effectiveTime = (int)(expTime - qrCodeExp.ElapsedMilliseconds / 1000);
                        loginStatus.QrCodeEffectiveTime = effectiveTime;

                        if (qrCodeExp.ElapsedMilliseconds / 1000 % 3 == 0)
                        {
                            if (loginStatus.IsScaned)
                            {
                                _logger.LogInformation($"已扫描，请确认登录。二维码剩余时长：{effectiveTime}s");
                            }
                            else
                            {
                                _logger.LogInformation($"等待扫描中...二维码剩余时长：{effectiveTime}s");
                            }
                            var scanResult = await QrCodeScanStatus(qrCodeInfo.qrcode_key);
                            if (scanResult.Data.status == QrCodeStatus.Scaned)
                            {
                                _logger.LogInformation("已执行登录确认。");
                                await _cookieService.SaveCookie(scanResult.Cookies, scanResult.Data.refresh_token);
                                userInfo = await LoginByCookie();
                                if (userInfo == null)
                                {
                                    throw new Exception("获取用户信息失败");
                                }
                                break;
                            }
                            if (scanResult.Data.status == QrCodeStatus.ScanedWithoutLogin && !loginStatus.IsScaned)
                            {
                                _logger.LogInformation("二维码已扫描。");
                                loginStatus.IsScaned = true;
                            }
                            else if (scanResult.Data.status == QrCodeStatus.Expired)
                            {
                                _logger.LogInformation("二维码已失效。");
                                break;
                            }
                        }
                        await Task.Delay(1000);
                    }

                    qrCodeExp.Stop();
                    hasCookie = _cookieService.HasCookie();
                    genIndex++;
                }

                if (!hasCookie && stopwatch.ElapsedMilliseconds >= timeout)
                {
                    throw new Exception("登录超时。");
                }
                stopwatch.Stop();
            }
            catch (Exception ex)
            {
                _logger.LogError($"扫码登录失败，{ex.Message}");
            }
            finally
            {
                _cache.Remove(CacheKeyConstant.LOGIN_STATUS_CACHE_KEY);
            }
            return userInfo;
        }

        public bool TryGetQrCodeLoginStatus(out QrCodeLoginStatus loginStatus)
        {
            bool result = _cache.TryGetValue(CacheKeyConstant.LOGIN_STATUS_CACHE_KEY, out loginStatus);
            if (result && loginStatus != null)
            {
                return result;
            }
            return false;
        }

        public async Task<bool> RefreshCookie()
        {
            if (!_cookieService.HasCookie())
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
                csrf = _cookieService.GetCsrf(),
                refresh_csrf = refreshCsrf,
                refresh_token = _cookieService.GetRefreshToken()
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
            await _cookieService.SaveCookie(refreshCookieResult.Cookies, refreshCookieResult.Data.refresh_token);
            //确认刷新cookie
            ConfirmRefreshModel confirmRefreshModel = new ConfirmRefreshModel()
            {
                csrf = _cookieService.GetCsrf(),
                refresh_token = refreshCookieModel.refresh_token,
            };
            ResultModel<object> confirmRefreshResult = await _httpClient.Execute<object>(_confirmRefresh, HttpMethod.Post, confirmRefreshModel, BodyFormat.Form_UrlEncoded);
            if (confirmRefreshResult == null || confirmRefreshResult.Code != 0)
            {
                throw new Exception($"刷新cookie失，确认更新失败。{confirmRefreshResult?.Message}");
            }
            return true;
        }

        public async Task<bool> CookieNeedToRefresh()
        {
            var result = await _httpClient.Execute<CookieInfo>(_cookieInfo, HttpMethod.Get);
            if (result == null || result.Code != 0 || result.Data == null)
            {
                throw new Exception($"获取Cookie信息失败！{result?.Message}");
            }
            return result.Data.refresh;
        }

        public async Task<QrCodeUrl> GenerateQrCode()
        {
            var result = await _httpClient.Execute<QrCodeUrl>(_generateQrCodeApi, HttpMethod.Get, withCookie: false);
            if (result == null || result.Data == null)
            {
                throw new Exception("生成登录二维码失败，返回内容为空！");
            }
            if (string.IsNullOrEmpty(result.Data.qrcode_key) || string.IsNullOrEmpty(result.Data.url))
            {
                throw new Exception("生成登录二维码信息失败");
            }
            return result.Data;
        }

        public async Task<ResultModel<QrCodeScanResult>> QrCodeScanStatus(string qrCodeKey)
        {
            if (string.IsNullOrWhiteSpace(qrCodeKey)) throw new ArgumentNullException(nameof(qrCodeKey));
            ResultModel<QrCodeScanResult> result = await _httpClient.Execute<QrCodeScanResult>(string.Format(_qrCodeHasScanedApi, qrCodeKey), HttpMethod.Get, withCookie: false);
            if (result == null || result.Data == null)
            {
                throw new Exception("获取{}二维码是否扫描结果为空！");
            }
            return result;
        }

        public async Task HeartBeat()
        {
            if (!_cookieService.HasCookie())
            {
                _logger.LogWarning($"心跳请求失败，未登录");
                return;
            }
            var result = await _httpClient.Execute<ResultModel<HeartBeat>>(_heartBeatApi, HttpMethod.Get);
            if (result == null || result.Code != 0)
            {
                _logger.LogWarning($"心跳请求失败，错误代码：{result.Code}，{result.Message}");
            }
        }

        #region private

        private string GetCorrespondPath()
        {
            var publicKeyPEM = @"
            -----BEGIN PUBLIC KEY-----
            MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDLgd2OAkcGVtoE3ThUREbio0Eg
            Uc/prcajMKXvkCKFCWhJYJcLkcM2DKKcSeFpD/j6Boy538YXnR6VhcuUJOhH2x71
            nzPjfdTcqMz7djHum0qSZA0AyCBDABUqCrfNgCiJ00Ra7GmRj+YCK1NJEuewlb40
            JNrRuoEUXpabUzGB8QIDAQAB
            -----END PUBLIC KEY-----";

            string ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
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

        /// <summary>
        /// 获取监听地址
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private List<string> GetEndpoint()
        {
            var address = _server.Features.Get<IServerAddressesFeature>()?.Addresses?.ToArray();
            if (address == null || address.Length == 0)
            {
                throw new Exception("Can not get current app endpoint.");
            }
            List<string> urls = new List<string>();
            var pattern = @"^(?<scheme>https?):\/\/((\+)|(\*)|\[::\]|(0.0.0.0))(?=[\:\/]|$)";
            foreach (var endpoint in address)
            {
                if (endpoint.Contains("localhost") || endpoint.Contains("127.0.0.1"))
                {
                    urls.Add(endpoint);
                    continue;
                }
                Match match = Regex.Match(endpoint, pattern);
                if (!match.Success)
                {
                    continue;
                }
                var localIpaddress = GetLocalNetworkAddress();
                if (localIpaddress?.Any() != true)
                {
                    continue;
                }
                foreach (var ipItem in localIpaddress)
                {
                    var uri = Regex.Replace(endpoint, pattern, "${scheme}://" + ipItem);
                    Uri httpEndpoint = new Uri(uri, UriKind.Absolute);
                    string url = new UriBuilder(httpEndpoint.Scheme, httpEndpoint.Host, httpEndpoint.Port)
                        .ToString()
                        .Replace(":80/", "")
                        .Replace(":443/", "")
                        .TrimEnd('/');
                    urls.Add(url);
                }
            }
            return urls;
        }

        /// <summary>
        /// 获取本机地址
        /// </summary>
        /// <returns></returns>
        private List<string> GetLocalNetworkAddress()
        {
            try
            {
                // 获取本地计算机上的所有网络接口
                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                    ?.Where(p => p.Supports(NetworkInterfaceComponent.IPv4) && p.OperationalStatus == OperationalStatus.Up)
                    .ToArray();
                if (networkInterfaces?.Any() != true)
                    return new List<string>();
                List<UnicastIPAddressInformation> allIps = new List<UnicastIPAddressInformation>();
                foreach (NetworkInterface networkInterface in networkInterfaces)
                {
                    // 获取IPv6地址信息
                    var ipAddresses = networkInterface.GetIPProperties()?.UnicastAddresses?
                        .Where(p => p.Address.AddressFamily == AddressFamily.InterNetwork && !p.Address.ToString().StartsWith("127"))
                        .ToArray();
                    if (ipAddresses?.Any() != true) continue;
                    allIps.AddRange(ipAddresses);
                }

                if (!allIps.Any())
                {
                    return new List<string>();
                }
                return allIps.Select(p => p.Address.ToString())
                    .OrderBy(p => p)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取本机IP地址失败，错误：{ex.Message}");
            }
            return null;
        }

        #endregion
    }
}
