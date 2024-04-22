using Bilibili.AspNetCore.Apis.Models.Base;
using Bilibili.AspNetCore.Apis.Models.Enums;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bilibili.AspNetCore.Apis.Interface
{
    public interface IHttpClientService
    {
        Task<ResultModel<T>> Execute<T>(string url, HttpMethod method, object body = null, BodyFormat format = BodyFormat.Json, bool withCookie = true, bool getRowData = false) where T : class;
    }
}
