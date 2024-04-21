using BilibiliAutoLiver.Models.Base;
using BilibiliAutoLiver.Models.Enums;
using System.Net.Http;
using System.Threading.Tasks;

namespace BilibiliAutoLiver.Services.Interface
{
    public interface IHttpClientService
    {
        Task<ResultModel<T>> Execute<T>(string url, HttpMethod method, object body = null, BodyFormat format = BodyFormat.Json, bool withCookie = true, bool getRowData = false) where T : class;
    }
}
