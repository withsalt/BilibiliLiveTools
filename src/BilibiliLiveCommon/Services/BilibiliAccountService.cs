using BilibiliLiveCommon.Model;
using BilibiliLiveCommon.Model.Base;
using BilibiliLiveCommon.Services.Interface;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Wasmtime;

namespace BilibiliLiveCommon.Services
{
    public class BilibiliAccountService : IBilibiliAccountService
    {
        private string _navApi = "https://api.bilibili.com/x/web-interface/nav";

        private const string _heartBeatApi = "https://api.live.bilibili.com/relation/v1/Feed/heartBeat";

        private const string _refeshToken = "https://passport.bilibili.com/api/oauth2/refreshToken";

        private const string _cookieInfo = "https://passport.bilibili.com/x/passport-login/web/cookie/info";

        private const string _getRefreshCsrf = "https://www.bilibili.com/correspond/1/{0}";

        private readonly IHttpClientService _httpClient;
        private readonly ILogger<BilibiliAccountService> _logger;
        private readonly IBilibiliCookieService _cookieService;

        public BilibiliAccountService(ILogger<BilibiliAccountService> logger
            , IHttpClientService httpClient
            , IBilibiliCookieService cookieService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _cookieService = cookieService ?? throw new ArgumentNullException(nameof(cookieService));
        }

        public async Task<UserInfo> Login()
        {
            var result = await _httpClient.Execute<ResultModel<UserInfo>>(_navApi, HttpMethod.Get);
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

        public async Task<bool> Refesh()
        {
            try
            {
                CookieHeaderValue values = _cookieService.CookieDeserialize(_cookieService.Get());
                var result = await _httpClient.Execute<ResultModel<CookieInfo>>(_cookieInfo, HttpMethod.Get);
                if (result == null || result.Code != 0)
                {
                    _logger.LogWarning($"刷新cookie失败，无法获取Cookie信息，错误代码：{result.Code}，{result.Message}");
                    return false;
                }
                string correspond = await GetCorrespondPathBackup(result.Data.timestamp.ToString());
                if (string.IsNullOrWhiteSpace(correspond))
                {
                    _logger.LogWarning($"刷新cookie失败，获取CorrespondPath失败");
                    return false;
                }
                string refeshUrl = string.Format(_getRefreshCsrf, correspond);
                string resultHtml = await _httpClient.Get(refeshUrl, false);

                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public async Task HeartBeat()
        {
            var result = await _httpClient.Execute<ResultModel<HeartBeat>>(_heartBeatApi, HttpMethod.Get);
            if (result == null || result.Code != 0)
            {
                _logger.LogWarning($"心跳请求失败，错误代码：{result.Code}，{result.Message}");
            }
        }

        private const string publicKey = @"-----BEGIN PUBLIC KEY----- MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDLgd2OAkcGVtoE3ThUREbio0Eg Uc/prcajMKXvkCKFCWhJYJcLkcM2DKKcSeFpD/j6Boy538YXnR6VhcuUJOhH2x71 nzPjfdTcqMz7djHum0qSZA0AyCBDABUqCrfNgCiJ00Ra7GmRj+YCK1NJEuewlb40 JNrRuoEUXpabUzGB8QIDAQAB -----END PUBLIC KEY-----";

        public string GetCorrespondPath(string timestamp)
        {
            try
            {
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportFromPem(publicKey);
                    byte[] data = Encoding.UTF8.GetBytes($"refresh_{timestamp}");
                    byte[] encryptedData = rsa.Encrypt(data, true);
                    return BitConverter.ToString(encryptedData).Replace("-", "").ToLower();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取timestamp {timestamp} hash失败！");
                return null;
            }
        }

        private async Task<string> GetCorrespondPathBackup(string timestamp)
        {
            try
            {
                string url = $"https://wasm-rsa.vercel.app/api/rsa?t={timestamp}";
                GetCorrespondResult correspondResult = await _httpClient.Execute<GetCorrespondResult>(url, HttpMethod.Get, withCookie: false);
                if(correspondResult != null && correspondResult.code == 0)
                {
                    return correspondResult.hash;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取timestamp {timestamp} hash失败！");
                return null;
            }
        }
    }
}
