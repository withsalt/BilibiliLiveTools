using BilibiliLiver.Model;
using BilibiliLiver.Model.Base;
using BilibiliLiver.Services.Interface;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BilibiliLiver.Services
{
    public class AccountService : IAccountService
    {
        private string _navApiUrl = "https://api.bilibili.com/x/web-interface/nav";

        private readonly IHttpClientService _httpClient;
        private readonly ILogger<AccountService> _logger;

        public AccountService(ILogger<AccountService> logger
            , IHttpClientService httpClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<UserInfo> Login()
        {
            var result = await _httpClient.Execute<ResultModel<UserInfo>>(_navApiUrl, RestSharp.Method.Get);
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
    }
}
