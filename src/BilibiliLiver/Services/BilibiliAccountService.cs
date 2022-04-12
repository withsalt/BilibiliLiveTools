using BilibiliLiver.Model;
using BilibiliLiver.Model.Base;
using BilibiliLiver.Services.Interface;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BilibiliLiver.Services
{
    public class BilibiliAccountService : IBilibiliAccountService
    {
        private string _navApi = "https://api.bilibili.com/x/web-interface/nav";

        private const string _heartBeatApi = "https://api.live.bilibili.com/relation/v1/Feed/heartBeat";

        private readonly IHttpClientService _httpClient;
        private readonly ILogger<BilibiliAccountService> _logger;

        public BilibiliAccountService(ILogger<BilibiliAccountService> logger
            , IHttpClientService httpClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<UserInfo> Login()
        {
            var result = await _httpClient.Execute<ResultModel<UserInfo>>(_navApi, RestSharp.Method.Get);
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

        public async Task HeartBeat()
        {
            var result = await _httpClient.Execute<ResultModel<HeartBeat>>(_heartBeatApi, RestSharp.Method.Get);
            if (result == null || result.Code != 0)
            {
                _logger.LogWarning($"心跳请求失败，错误代码：{result.Code}，{result.Message}");
            }
        }
    }
}
