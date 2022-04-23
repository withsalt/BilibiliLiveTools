using System;
using System.Net.Http;

namespace BilibiliLiveCommon.Model.Exceptions
{
    public class ApiRequestException : Exception
    {
        public string Api { get; set; }

        public HttpMethod Method { get; set; }

        public ApiRequestException(string api, HttpMethod method, string message) : base($"接口请求失败，接口：{api}，方法：{method}，描述：{message}")
        {
            Api = api;
            Method = method;
        }

        public ApiRequestException(string api, HttpMethod method, string message, Exception innerException) : base($"接口请求失败，接口：{api}，方法：{method}，，描述：{message},错误：{innerException.Message}", innerException)
        {
            Api = api;
            Method = method;
        }
    }
}
