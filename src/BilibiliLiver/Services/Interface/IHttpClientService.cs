using BilibiliLiver.Model.Enums;
using System.Net.Http;
using System.Threading.Tasks;

namespace BilibiliLiver.Services.Interface
{
    public interface IHttpClientService
    {
        Task<T> Execute<T>(string url, HttpMethod method, object body = null, BodyFormat format = BodyFormat.Json, bool withCookie = true) where T : class;
    }
}
