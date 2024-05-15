using System.Threading;
using System.Threading.Tasks;

namespace BilibiliAutoLiver.Services.Interface
{
    interface IStartupService
    {
        Task Start(CancellationToken token);
    }
}
