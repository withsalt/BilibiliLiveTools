using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Models.Base;
using Bilibili.AspNetCore.Apis.Models.Enums;

namespace Bilibili.AspNetCore.Apis.Interface
{
    public interface IHttpClientService
    {
        Task<ResultModel<T>> GetAsync<T>(string url, bool rowData = false, CancellationToken cancellationToken = default) where T : class;

        Task<ResultModel<T>> GetWithoutPermissionAsync<T>(string url, bool rowData = false, CancellationToken cancellationToken = default) where T : class;

        Task<ResultModel<T>> PostAsync<T>(string url, object body, BodyFormat format, CancellationToken cancellationToken = default) where T : class;
    }
}
