using BilibiliLiver.Model;
using BilibiliLiver.Utils;
using Microsoft.Extensions.Logging;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLiver.Services
{
    public class HttpClientService : IHttpClientService
    {
        private readonly ILogger<HttpClientService> _logger;
        private readonly IBilibiliCookieService _bilibiliCookie;

        public HttpClientService(ILogger<HttpClientService> logger
            , IBilibiliCookieService bilibiliCookie)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _bilibiliCookie = bilibiliCookie ?? throw new ArgumentNullException(nameof(bilibiliCookie));
        }

        public async Task<T> Execute<T>(string url, Method method, object body = null) where T : class
        {
            var client = new RestClient(new RestClientOptions()
            {
                RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true
            });
            var request = new RestRequest(url, method)
            {
                Timeout = 3000,
            };
            if(request.Method != Method.Get && body != null)
            {
                request.AddBody(body);

                //request.AddParameter("", "", ParameterType.RequestBody);

                request.AddHeader("Content-Type", "multipart/form-data");
            }
            request.AddHeader("origin", "https://www.bilibili.com");
            request.AddHeader("referer", "https://www.bilibili.com/");
            request.AddHeader("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.75 Safari/537.36");
            request.AddHeader("accept", "*/*");
            request.AddHeader("cookie", _bilibiliCookie.Get());

            var result = await client.ExecuteAsync(request);
            if (result.StatusCode != HttpStatusCode.OK || string.IsNullOrEmpty(result.Content))
            {
                throw new Exception($"Http request failed, url is {url}, method is {method}. http status code is {result.StatusCode}, result is {result.Content}");
            }
            return JsonUtil.DeserializeJsonToObject<T>(result.Content);
        }

        private Dictionary<string, string> DeserializeObjectToDictionary(object obj)
        {
            return new Dictionary<string, string>();
        }
    }
}
