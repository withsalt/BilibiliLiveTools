using BilibiliLiveCommon.Model;
using BilibiliLiveCommon.Model.Base;
using BilibiliLiveCommon.Services.Interface;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

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

        private readonly IHttpClientService _httpClient;
        private readonly ILogger<BilibiliAccountService> _logger;

        public BilibiliAccountService(ILogger<BilibiliAccountService> logger
            , IHttpClientService httpClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<UserInfo> LoginByCookie()
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
    }
}
