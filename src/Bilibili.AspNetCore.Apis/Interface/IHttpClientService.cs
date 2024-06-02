using System.Net.Http;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Models.Base;
using Bilibili.AspNetCore.Apis.Models.Enums;

namespace Bilibili.AspNetCore.Apis.Interface
{
    internal interface IHttpClientService
    {
        Task<ResultModel<T>> Execute<T>(string url, HttpMethod method, object body = null, BodyFormat format = BodyFormat.Json, bool withCookie = true, string cookie = null, bool getRowData = false) where T : class;
    }
}
