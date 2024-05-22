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
        private readonly MonitorClientJob _monitorClientJob;

        public JobSchedulerService(ILogger<JobSchedulerService> logger
            , ISchedulerFactory schedulerFactory
            , RefreshCookieJob refreshCookieJob
            , SendHeartBeatJob sendHeartBeatJob
            , MonitorClientJob monitorClientJob)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _schedulerFactory = schedulerFactory ?? throw new ArgumentNullException(nameof(schedulerFactory));
            //create scheduler
            _scheduler = _schedulerFactory.GetScheduler().GetAwaiter().GetResult();
            if (_scheduler == null)
            {
                throw new Exception("Can not get scheduler from scheduler factory.");
            }
            //jobs
            _refreshCookieJob = refreshCookieJob ?? throw new ArgumentNullException(nameof(refreshCookieJob));
            _sendHeartBeatJob = sendHeartBeatJob ?? throw new ArgumentNullException(nameof(sendHeartBeatJob));
            _monitorClientJob = monitorClientJob ?? throw new ArgumentNullException(nameof(monitorClientJob));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _scheduler.ScheduleJob(_sendHeartBeatJob.JobDetail, _sendHeartBeatJob.CreateTrigger(), cancellationToken);
            await _scheduler.ScheduleJob(_refreshCookieJob.JobDetail, _refreshCookieJob.CreateTrigger(), cancellationToken);
            await _scheduler.ScheduleJob(_monitorClientJob.JobDetail, _monitorClientJob.CreateTrigger(), cancellationToken);
            await _scheduler.Start(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _scheduler.Shutdown(cancellationToken);
        }
    }
}
