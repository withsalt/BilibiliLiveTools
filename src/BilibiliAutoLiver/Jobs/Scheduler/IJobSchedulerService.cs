using System;
using System.Threading;
using System.Threading.Tasks;
using BilibiliAutoLiver.Jobs.Job;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BilibiliAutoLiver.Jobs.Scheduler
{
    public interface IJobSchedulerService
    {
        Task StartAsync(CancellationToken cancellationToken);

        Task StopAsync(CancellationToken cancellationToken);
    }
}
