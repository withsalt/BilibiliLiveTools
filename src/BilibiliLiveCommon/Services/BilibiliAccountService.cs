using BilibiliLiveCommon.Model;
using BilibiliLiveCommon.Model.Base;
using BilibiliLiveCommon.Services.Interface;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace BilibiliLiveCommon.Services
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
        private readonly IBilibiliCookieService _bilibiliCookie;
        private readonly ILogger<BilibiliAccountService> _logger;

        public BilibiliAccountService(ILogger<BilibiliAccountService> logger
            , IHttpClientService httpClient
            , IBilibiliCookieService bilibiliCookie)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _bilibiliCookie = bilibiliCookie ?? throw new ArgumentNullException(nameof(bilibiliCookie));
        }

        public async Task<UserInfo> GetUserInfo()
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

        public async Task<UserInfo> LoginByCookie()
        {
            return await GetUserInfo();
        }

        public Task<bool> LoginQrCode()
        {

            return Task.FromResult(true);
        }

        public async Task<bool> RefreshCookie()
        {
            if (!_bilibiliCookie.HasCookie())
            {
                throw new Exception("未登录，请先登录！");
            }
            string key = GetCorrespondPath();
            var refreshCsrfRt = await _httpClient.Execute<string>(string.Format(_getRefreshCsrf, key), HttpMethod.Get, getRowData: true);
            if (string.IsNullOrWhiteSpace(refreshCsrfRt.RowData))
            {
                throw new Exception($"获取refresh csrf失败！");
            }

            string pattern = @"<div id=""1-name"">(?<content>.*?)</div>";
            Match match = Regex.Match(refreshCsrfRt.RowData, pattern);
            if (!match.Success)
            {
                throw new Exception($"获取refresh csrf失败，返回内容：{refreshCsrfRt.RowData}");
            }
            string refreshCsrf = match.Groups["content"]?.Value;
            if (string.IsNullOrWhiteSpace(refreshCsrf))
            {
                throw new Exception($"获取refresh csrf失败");
            }
            //刷新cookie
            RefreshCookieModel refreshCookieModel = new RefreshCookieModel()
            {
                csrf = _bilibiliCookie.GetCsrf(),
                refresh_csrf = refreshCsrf,
                refresh_token = _bilibiliCookie.GetRefreshToken()
            };
            ResultModel<RefreshCookieResult> refreshCookieResult = await _httpClient.Execute<RefreshCookieResult>(_refreshCookie, HttpMethod.Post, refreshCookieModel, Model.Enums.BodyFormat.Form_UrlEncoded);
            if (refreshCookieResult == null || string.IsNullOrWhiteSpace(refreshCookieResult?.Data.refresh_token) || refreshCookieResult.Cookies?.Any() != true)
            {
                throw new Exception($"刷新cookie失败。{refreshCookieResult?.Message}");
            }
            await _bilibiliCookie.SaveCookie(refreshCookieResult.Cookies, refreshCookieResult.Data.refresh_token);
            //确认刷新cookie
            ConfirmRefreshModel confirmRefreshModel = new ConfirmRefreshModel()
            {
                csrf = _bilibiliCookie.GetCsrf(),
                refresh_token = refreshCookieModel.refresh_token,
            };
            ResultModel<object> confirmRefreshResult = await _httpClient.Execute<object>(_confirmRefresh, HttpMethod.Post, confirmRefreshModel, Model.Enums.BodyFormat.Form_UrlEncoded);
            if (confirmRefreshResult == null || confirmRefreshResult.Code != 0)
            {
                throw new Exception($"确认刷新cookie失败。{confirmRefreshResult?.Message}");
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

        public async Task<ResultModel<QrCodeScanResult>> QrCodeHasScaned(string qrCodeKey)
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

        #endregion
    }
}
