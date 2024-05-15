using System;
using BilibiliAutoLiver.Models;
using Quartz;

namespace BilibiliAutoLiver.Jobs.Interface
{
    public abstract class BaseJobDescribe: IJobDescribe
    {
        protected abstract JobMetadata JobMetadata { get; }

        public IJobDetail JobDetail
        {
            get
            {
                IJobDetail result = JobBuilder.Create(this.GetType())
                    .WithIdentity(new JobKey(this.JobMetadata.Id.ToString(), this.JobMetadata.Name))
                    .WithDescription(this.JobMetadata.Name)
                    .Build();
                result.JobDataMap.Put(nameof(this.JobMetadata.Id), this.JobMetadata.Id);
                result.JobDataMap.Put(nameof(this.JobMetadata.Name), this.JobMetadata.Name);
                return result;
            }
        }

        public ITrigger CreateTrigger()
        {
            if (this.JobMetadata.EndTime != null && this.JobMetadata.EndTime < DateTime.Now)
            {
                throw new Exception("任务结束时间不能小于当前时间");
            }
            if (this.JobMetadata.StartTime != null && this.JobMetadata.EndTime != null && this.JobMetadata.EndTime.Value < this.JobMetadata.StartTime.Value)
            {
                throw new Exception("任务结束时间必须大于任务开始时间");
            }
            DateTime nowTime = DateTime.Now;
            DateTime? startTime = this.JobMetadata.StartTime;
            var triggerBuilder = TriggerBuilder.Create()
                .WithIdentity($"{this.JobMetadata.Id}_trigger", this.JobMetadata.Name)
                .WithDescription(this.JobMetadata.Description)
                .WithSimpleSchedule(x => x.RepeatForever()
                    .WithIntervalInSeconds(this.JobMetadata.IntervalTime)
                );
            if (startTime != null)
            {
                if (startTime.Value < nowTime)
                    startTime = nowTime;
                if (startTime < nowTime.AddSeconds(3))
                    startTime = startTime.Value.AddSeconds(3);
                triggerBuilder.StartAt(startTime.Value);
            }
            if (this.JobMetadata.EndTime != null)
            {
                triggerBuilder.EndAt(this.JobMetadata.EndTime.Value);
            }
            return triggerBuilder.Build();
        }
    }
}
