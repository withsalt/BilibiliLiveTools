using BilibiliLiver.Model.Enums;
using BilibiliLiver.Services.Interface;
using BilibiliLiver.Utils;
using Microsoft.Extensions.Logging;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
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

        public async Task<T> Execute<T>(string url, Method method, object body = null, BodyFormat format = BodyFormat.Json) where T : class
        {
            using (var client = new RestClient(new RestClientOptions()
            {
                RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true
            }))
            {
                var request = new RestRequest(url, method)
                {
                    Timeout = 6000,
                };
                if (request.Method != Method.Get && body != null)
                {
                    switch (format)
                    {
                        case BodyFormat.Json:
                            {
                                request.RequestFormat = DataFormat.Json;
                                request.AddBody(body);
                            }
                            break;
                        case BodyFormat.Form:
                            {
                                Dictionary<string, string> @params = ObjectToMap(body);
                                foreach (var item in @params)
                                {
                                    request.AddParameter(item.Key, item.Value, ParameterType.GetOrPost, false);
                                }
                                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                            }
                            break;
                    }
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
                string data = result.Content.Replace("\"data\":[]", "\"data\":null");
                return JsonUtil.DeserializeJsonToObject<T>(data);
            }
        }

        private Dictionary<string, string> ObjectToMap(object obj, bool isIgnoreNull = false)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            if (obj == null)
            {
                return map;
            }
            Type t = obj.GetType();
            PropertyInfo[] pi = t.GetProperties(BindingFlags.Public | BindingFlags.Instance); // 获取当前type公共属性
            foreach (PropertyInfo p in pi)
            {
                MethodInfo m = p.GetGetMethod();
                if (m != null && m.IsPublic)
                {
                    // 进行判NULL处理 
                    if (m.Invoke(obj, new object[] { }) != null || !isIgnoreNull)
                    {
                        map.Add(p.Name, m.Invoke(obj, new object[] { }).ToString()); // 向字典添加元素
                    }
                }
            }
            return map;
        }
    }
}
