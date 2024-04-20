using System.Threading.Tasks;

namespace BilibiliAutoLiver.Jobs.Scheduler
{
    interface IRefreshCookieJobSchedulerService
    {
        Task Start();
    }
}
