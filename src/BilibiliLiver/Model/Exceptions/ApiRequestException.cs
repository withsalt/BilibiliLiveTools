using RestSharp;
using System;

namespace BilibiliLiver.Model.Exceptions
{
    public class ApiRequestException : Exception
    {
        public string Api { get; set; }

        public Method Method { get; set; }

        public ApiRequestException(string api, Method method, string message) : base($"接口请求失败，接口：{api}，方法：{method}，描述：{message}")
        {
            Api = api;
            Method = method;
        }

        public ApiRequestException(string api, Method method, string message, Exception innerException) : base($"接口请求失败，接口：{api}，方法：{method}，，描述：{message},错误：{innerException.Message}", innerException)
        {
            Api = api;
            Method = method;
        }
    }
}
