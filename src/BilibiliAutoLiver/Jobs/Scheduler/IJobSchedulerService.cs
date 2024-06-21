using System.Threading;
using System.Threading.Tasks;

namespace BilibiliAutoLiver.Jobs.Scheduler
{
    public interface IJobSchedulerService
    {
        Task Start(CancellationToken cancellationToken);

        Task Stop(CancellationToken cancellationToken);
    }
}
