using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Constants;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models.Base;
using Bilibili.AspNetCore.Apis.Models.Enums;
using Bilibili.AspNetCore.Apis.Utils;

namespace Bilibili.AspNetCore.Apis.Services
{
    internal class HttpClientService : IHttpClientService
    {
        private readonly IBilibiliCookieService _cookie;

        public HttpClientService(IBilibiliCookieService cookie)
        {
            _cookie = cookie ?? throw new ArgumentNullException(nameof(cookie));
        }

        public async Task<ResultModel<T>> Execute<T>(string url, HttpMethod method, object body = null, BodyFormat format = BodyFormat.Json, bool withCookie = true, string cookie = null, bool getRowData = false) where T : class
        {
            using (HttpClient httpClient = new HttpClient(new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = delegate { return true; },
                AutomaticDecompression = DecompressionMethods.GZip
            })
            {
                Timeout = TimeSpan.FromSeconds(60)
            })
            {
                httpClient.DefaultRequestHeaders.Add("accept", "*/*");
                httpClient.DefaultRequestHeaders.Add("accept-language", "zh-CN,zh;q=0.9");
                httpClient.DefaultRequestHeaders.Add("origin", "https://www.bilibili.com");
                httpClient.DefaultRequestHeaders.Add("priority", "u=1, i");
                httpClient.DefaultRequestHeaders.Add("referer", "https://www.bilibili.com/");
                httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Chromium\";v=\"124\", \"Google Chrome\";v=\"124\", \"Not-A.Brand\";v=\"99\"");
                httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                httpClient.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
                httpClient.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
                httpClient.DefaultRequestHeaders.Add("sec-fetch-site", "same-site");
                httpClient.DefaultRequestHeaders.Add("user-agent", GlobalConfigConstant.USER_AGENT);
                httpClient.DefaultRequestHeaders.Add("cache-control", "no-cache");

                if (withCookie)
                {
                    if (!string.IsNullOrWhiteSpace(cookie))
                        httpClient.DefaultRequestHeaders.Add("cookie", cookie);
                    else
                        httpClient.DefaultRequestHeaders.Add("cookie", _cookie.GetString());
                }

                HttpResponseMessage response = null;
                try
                {
                    if (method == HttpMethod.Post)
                    {
                        if (body == null)
                        {
                            throw new ArgumentNullException(nameof(body));
                        }
                        switch (format)
                        {
                            case BodyFormat.Json:
                                {
                                    string postData = null;
                                    if (body.GetType() == typeof(string))
                                    {
                                        postData = body.ToString();
                                    }
                                    else
                                    {
                                        postData = JsonUtil.SerializeObject(body);
                                    }
                                    using (StringContent content = new StringContent(postData))
                                    {
                                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(content.Headers.ContentType.MediaType));
                                        response = await httpClient.PostAsync(url, content);
                                    }
                                }
                                break;
                            case BodyFormat.Form:
                                {
                                    if (body.GetType() == typeof(string))
                                    {
                                        throw new Exception("When post body format is form, body can not string.");
                                    }
                                    Dictionary<string, string> @params = ObjectToMap(body);
                                    if (@params.Count == 0)
                                    {
                                        throw new ArgumentNullException("Cannot convert body to dictionary data.", nameof(body));
                                    }
                                    using (MultipartFormDataContent content = new MultipartFormDataContent())
                                    {
                                        foreach (var item in @params)
                                        {
                                            content.Add(new StringContent(item.Value), item.Key);
                                        }
                                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(content.Headers.ContentType.MediaType));
                                        response = await httpClient.PostAsync(url, content);
                                    }
                                }
                                break;
                            case BodyFormat.Form_UrlEncoded:
                                {
                                    if (typeof(bool) == typeof(string))
                                    {
                                        throw new Exception("When post body format is form-urlencoded, body can not string.");
                                    }
                                    Dictionary<string, string> @params = ObjectToMap(body);
                                    using (FormUrlEncodedContent content = new FormUrlEncodedContent(@params))
                                    {
                                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(content.Headers.ContentType.MediaType));
                                        response = await httpClient.PostAsync(url, content);
                                    }
                                }
                                break;
                        }
                    }
                    else if (method == HttpMethod.Get)
                    {
                        response = await httpClient.GetAsync(url);
                    }
                    else
                    {
                        throw new Exception($"Not support http method: {method}");
                    }
                    if (response == null)
                    {
                        throw new Exception($"Http request failed, url: {url}, method: {method},  result is null.");
                    }
                    string resultStr = await TryGetStringResponse(response);
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception($"Http request failed, url: {url}, method: {method}, status code: {response.StatusCode}, result: {(string.IsNullOrEmpty(resultStr) || method == HttpMethod.Get ? "null" : resultStr)}");
                    }
                    response.Dispose();
                    string data = resultStr.Replace("\"data\":[]", "\"data\":null");
                    ResultModel<T> resultObj = null;
                    if (getRowData)
                    {
                        resultObj = new ResultModel<T>()
                        {
                            Code = 0,
                            Message = "Success",
                            Data = resultStr as T
                        };
                    }
                    else
                    {
                        resultObj = JsonUtil.DeserializeJsonToObject<ResultModel<T>>(data);
                    }
                    //set cookie
                    IEnumerable<string> setCookies = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
                    if (setCookies?.Any() == true)
                    {
                        List<CookieHeaderValue> cookies = new List<CookieHeaderValue>();
                        foreach (var item in setCookies)
                        {
                            if (CookieHeaderValue.TryParse(item, out var cookieItem))
                            {
                                cookies.Add(cookieItem);
                            }
                        }
                        resultObj.Cookies = cookies;
                    }
                    return resultObj;
                }
                finally
                {
                    if (response != null) response.Dispose();
                }
            }
        }

        /// <summary>
        /// Object转化为dictionory
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="isIgnoreNull"></param>
        /// <returns></returns>
        private Dictionary<string, string> ObjectToMap(object obj, bool isIgnoreNull = false)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            if (obj == null)
            {
                return map;
            }
            Type t = obj.GetType();
            if (obj is Dictionary<string, string>)
            {
                var paramDic = obj as Dictionary<string, string>;
                if (paramDic?.Any() == true)
                {
                    return paramDic;
                }
                return map;
            }
            PropertyInfo[] pi = t.GetProperties(BindingFlags.Public | BindingFlags.Instance); // 获取当前type公共属性
            foreach (PropertyInfo p in pi)
            {
                MethodInfo m = p.GetGetMethod();
                if (m != null && m.IsPublic)
                {
                    // 进行判NULL处理 
                    if (m.Invoke(obj, new object[] { }) != null || !isIgnoreNull)
                    {
                        string value = null;
                        if (m.ReturnType == typeof(String))
                            value = m.Invoke(obj, new object[] { }).ToString();
                        else
                            value = JsonUtil.SerializeObject(m.Invoke(obj, new object[] { }));
                        map.Add(p.Name, string.IsNullOrEmpty(value) ? null : value); // 向字典添加元素
                    }
                }
            }
            return map;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private async Task<string> TryGetStringResponse(HttpResponseMessage content)
        {
            if (content == null)
            {
                return null;
            }
            try
            {
                string resultStr = await content.Content.ReadAsStringAsync();
                return resultStr;
            }
            catch { }
            return null;
        }
    }
}
