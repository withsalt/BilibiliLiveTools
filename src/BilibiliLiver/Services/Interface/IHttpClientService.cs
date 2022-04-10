using BilibiliLiver.Model.Enums;
using RestSharp;
using System.Threading.Tasks;

namespace BilibiliLiver.Services.Interface
{
    public interface IHttpClientService
    {
        Task<T> Execute<T>(string url, Method method, object body = null, BodyFormat format = BodyFormat.Json) where T : class;
    }
}
