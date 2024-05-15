using System;
using System.Threading;
using System.Threading.Tasks;
using BilibiliAutoLiver.Jobs.Job;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BilibiliAutoLiver.Jobs.Scheduler
{
    public class JobSchedulerService : IJobSchedulerService
    {
        private readonly ILogger<JobSchedulerService> _logger;
        private readonly IScheduler _scheduler;
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly RefreshCookieJob _refreshCookieJob;
        private readonly SendHeartBeatJob _sendHeartBeatJob;

        public JobSchedulerService(ILogger<JobSchedulerService> logger
            , ISchedulerFactory schedulerFactory
            , RefreshCookieJob refreshCookieJob
            , SendHeartBeatJob sendHeartBeatJob)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _schedulerFactory = schedulerFactory ?? throw new ArgumentNullException(nameof(schedulerFactory));
            //create scheduler
            _scheduler = _schedulerFactory.GetScheduler().GetAwaiter().GetResult();
            if (_scheduler == null)
            {
                throw new Exception("Can not get scheduler from scheduler factory.");
            }
            _refreshCookieJob = refreshCookieJob ?? throw new ArgumentNullException(nameof(refreshCookieJob));
            _sendHeartBeatJob = sendHeartBeatJob ?? throw new ArgumentNullException(nameof(sendHeartBeatJob));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _scheduler.ScheduleJob(_sendHeartBeatJob.JobDetail, _sendHeartBeatJob.CreateTrigger(), cancellationToken);
            await _scheduler.ScheduleJob(_refreshCookieJob.JobDetail, _refreshCookieJob.CreateTrigger(), cancellationToken);
            await _scheduler.Start(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _scheduler.Shutdown(cancellationToken);
        }
    }
}
