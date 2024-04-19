using BilibiliLiveCommon.Model.Base;
using BilibiliLiveCommon.Model.Enums;
using System.Net.Http;
using System.Threading.Tasks;

namespace BilibiliLiveCommon.Services.Interface
{
    public interface IHttpClientService
    {
        Task<ResultModel<T>> Execute<T>(string url, HttpMethod method, object body = null, BodyFormat format = BodyFormat.Json, bool withCookie = true, bool getRowData = false) where T : class;
    }
}
