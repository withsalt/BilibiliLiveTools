using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Constants;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models.Base;
using Bilibili.AspNetCore.Apis.Models.Enums;
using Bilibili.AspNetCore.Apis.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Bilibili.AspNetCore.Apis.Services.Cookie
{
    internal class CookieBuilderHttpClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CookieBuilderHttpClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<ResultModel<T>> GetAsync<T>(string url, bool rowData = false, CancellationToken cancellationToken = default) where T : class
        {
            return await Execute<T>(url, HttpMethod.Get, null, BodyFormat.Json, null, cancellationToken);
        }

        public async Task<ResultModel<T>> PostAsync<T>(string url, object body, BodyFormat format, string cookie, CancellationToken cancellationToken = default) where T : class
        {
            return await Execute<T>(url, HttpMethod.Post, body, format, cookie, cancellationToken);
        }

        private async Task<ResultModel<T>> Execute<T>(string url, HttpMethod method, object body = null, BodyFormat format = BodyFormat.Json, string cookie = null, CancellationToken cancellationToken = default) where T : class
        {
            // 1. 创建 HttpRequestMessage，这是最佳实践
            using var request = new HttpRequestMessage(method, url);

            // 2. 处理 Cookie
            if (!string.IsNullOrWhiteSpace(cookie))
            {
                request.Headers.Add("cookie", cookie);
            }

            // 3. 处理请求体
            if (method == HttpMethod.Post && body != null)
            {
                request.Content = CreateHttpContent(body, format);
            }

            // 4. 发送请求
            var httpClient = _httpClientFactory.CreateClient("BilibiliRequestClient");
            using var response = await httpClient.SendAsync(request, cancellationToken);

            // 5. 处理响应
            return await ProcessResponse<T>(response, false, url);
        }

        /// <summary>
        /// 创建请求体内容
        /// </summary>
        private HttpContent CreateHttpContent(object body, BodyFormat format)
        {
            switch (format)
            {
                case BodyFormat.Json:
                    var jsonString = body is string s ? s : JsonUtil.SerializeObject(body);
                    return new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");

                case BodyFormat.Form_UrlEncoded:
                    if (body is string)
                    {
                        throw new ArgumentException("For Form-UrlEncoded format, body cannot be a raw string. It must be an object or a Dictionary.", nameof(body));
                    }
                    Dictionary<string, string> @params = ObjectToMap(body);
                    return new FormUrlEncodedContent(ObjectToMap(body));

                case BodyFormat.Form:
                    if (body is string)
                    {
                        throw new ArgumentException("For Form format, body cannot be a raw string. It must be an object or a Dictionary.", nameof(body));
                    }
                    var formData = new MultipartFormDataContent();
                    foreach (var kvp in ObjectToMap(body))
                    {
                        formData.Add(new StringContent(kvp.Value ?? string.Empty), kvp.Key);
                    }
                    return formData;

                default:
                    throw new NotSupportedException($"BodyFormat '{format}' is not supported.");
            }
        }

        /// <summary>
        /// 处理并解析HTTP响应
        /// </summary>
        private async Task<ResultModel<T>> ProcessResponse<T>(HttpResponseMessage response, bool getRowData, string requestUrl) where T : class
        {
            var resultStr = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Request to {requestUrl} failed. Status: {response.StatusCode}. Response: {resultStr}",
                    null,
                    response.StatusCode);
            }

            if (getRowData)
            {
                if (typeof(T) != typeof(string))
                {
                    throw new InvalidOperationException($"When 'getRowData' is true, the generic type T must be 'string', but it was '{typeof(T).Name}'.");
                }

                return new ResultModel<T>
                {
                    Code = 0,
                    Message = "Success",
                    Data = resultStr as T
                };
            }

            var resultObj = JsonUtil.DeserializeJsonToObject<ResultModel<T>>(resultStr);
            if (response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string> setCookies))
            {
                resultObj.Cookies = setCookies
                    .Select(c => CookieHeaderValue.TryParse(c, out var parsed) ? parsed : null)
                    .Where(c => c != null)
                    .ToList();
            }

            return resultObj;
        }

        /// <summary>
        /// 将对象通过反射转换为字典。
        /// 注意：反射有性能开销，对于高性能场景需要考虑缓存。
        /// </summary>
        private Dictionary<string, string> ObjectToMap(object obj)
        {
            if (obj == null) return new Dictionary<string, string>();

            if (obj is Dictionary<string, string> dict) return dict;

            var map = new Dictionary<string, string>();
            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                if (!prop.CanRead) continue;

                var value = prop.GetValue(obj);
                map[prop.Name] = value?.ToString();
            }
            return map;
        }
    }
}

