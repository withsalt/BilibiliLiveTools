using System.Threading;
using System.Threading.Tasks;

namespace BilibiliAutoLiver.Jobs.Scheduler
{
    public interface IJobSchedulerService
    {
        Task StartAsync(CancellationToken cancellationToken);

        Task StopAsync(CancellationToken cancellationToken);
    }
}
