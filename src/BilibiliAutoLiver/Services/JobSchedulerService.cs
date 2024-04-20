using System;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models;
using BilibiliLiveMonitor.Jobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace BilibiliAutoLiver.Services
{
    class JobSchedulerService : IJobSchedulerService
    {
        private readonly ILogger<JobSchedulerService> _logger;
        private readonly IScheduler _scheduler;
        private readonly ISchedulerFactory _schedulerFactory;

        public JobSchedulerService(ILogger<JobSchedulerService> logger
            , ISchedulerFactory schedulerFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            IJobDetail jobDetail = CreateJobDetail(job);
            ITrigger trigger = CreateTrigger(job);
            await _scheduler.ScheduleJob(jobDetail, trigger);
        }

        private JobDescribe CreateJobDescribe()
        {
            return new JobDescribe()
            {
                Id = 1,
                Name = nameof(RefreshCookieJob) + "Service",
                IntervalTime = 20,
                StartTime = DateTime.Now.AddSeconds(10)
            };
        }

        private IJobDetail CreateJobDetail(JobDescribe job)
        {
            IJobDetail result = JobBuilder.Create<RefreshCookieJob>()
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
