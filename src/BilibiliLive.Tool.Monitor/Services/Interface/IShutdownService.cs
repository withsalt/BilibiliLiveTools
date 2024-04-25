using System.Threading.Tasks;

namespace BilibiliLive.Tool.Monitor.Services.Interface
{
    public interface IShutdownService
    {
        Task<bool> Shutdown();
    }
}
