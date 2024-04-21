using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using BilibiliLiveMonitor.Configs;
using BilibiliLiveMonitor.Jobs;
using BilibiliLiveMonitor.Models;
using System;
using System.Threading.Tasks;

namespace BilibiliLiveMonitor.Services
{
    public class JobSchedulerService : IJobSchedulerService
    {
        private readonly ILogger<JobSchedulerService> _logger;
        private readonly AppSettingsNode _appSettings;
        private readonly IScheduler _scheduler;
        private readonly ISchedulerFactory _schedulerFactory;

        public JobSchedulerService(ILogger<JobSchedulerService> logger
            , ISchedulerFactory schedulerFactory
            , IOptions<AppSettingsNode> appSettingsOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appSettings = appSettingsOptions.Value ?? throw new ArgumentNullException(nameof(appSettingsOptions));
            _schedulerFactory = schedulerFactory ?? throw new ArgumentNullException(nameof(schedulerFactory));
            //create scheduler
            _scheduler = _schedulerFactory.GetScheduler().GetAwaiter().GetResult();
            if (_scheduler == null)
            {
                throw new Exception("Can not get scheduler from scheduler factory.");
            }
        }

        public async Task Start()
        {
            JobDescribe job = CreateJobDescribe();
            // define the job and tie it to our HelloJob class
            IJobDetail jobDetail = CreateJobDetail(job);
            // Trigger the job to run now, and then repeat every 10 seconds
            ITrigger trigger = CreateTrigger(job);
            // Tell quartz to schedule the job using our trigger
            await _scheduler.ScheduleJob(jobDetail, trigger);
        }

        private JobDescribe CreateJobDescribe()
        {
            int interval = _appSettings.IntervalTime;
            if (interval < 60 || interval > 43200)
            {
#if !DEBUG
                throw new Exception("The interval must be less than 43200 and greater than 60 seconds");
#endif
            }
            return new JobDescribe()
            {
                Id = 1,
                Name = nameof(MonitorClientJob) + "Service",
                IntervalTime = interval,
                StartTime = DateTime.Now.AddSeconds(3)
            };
        }

        private IJobDetail CreateJobDetail(JobDescribe job)
        {
            IJobDetail result = JobBuilder.Create<MonitorClientJob>()
                .WithIdentity(new JobKey(job.Id.ToString(), job.Name))
                .WithDescription(job.Name)
                .Build();
            result.JobDataMap.Put(nameof(job.Id), job.Id);
            result.JobDataMap.Put(nameof(job.Name), job.Name);
            return result;
        }

        private ITrigger CreateTrigger(JobDescribe job)
        {
            if (job.EndTime != null && job.EndTime < DateTime.Now)
            {
                throw new Exception("任务结束时间不能小于当前时间");
            }
            if (job.StartTime != null && job.EndTime != null && job.EndTime.Value < job.StartTime.Value)
            {
                throw new Exception("任务结束时间必须大于任务开始时间");
            }
            DateTime nowTime = DateTime.Now;
            DateTime? startTime = job.StartTime;
            var triggerBuilder = TriggerBuilder.Create()
                .WithIdentity($"{job.Id}_trigger", job.Name)
                .WithDescription(job.Description)
                .WithSimpleSchedule(x => x.RepeatForever()
                    .WithIntervalInSeconds(job.IntervalTime)
                );
            if (startTime != null)
            {
                if (startTime.Value < nowTime)
                    startTime = nowTime;
                if (startTime < nowTime.AddSeconds(3))
                    startTime = startTime.Value.AddSeconds(3);
                triggerBuilder.StartAt(startTime.Value);
            }
            if (job.EndTime != null)
            {
                triggerBuilder.EndAt(job.EndTime.Value);
            }
            return triggerBuilder.Build();
        }
    }
}
