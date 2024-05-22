using System;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models;
using BilibiliAutoLiver.Config;
using BilibiliAutoLiver.Jobs.Interface;
using BilibiliAutoLiver.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BilibiliAutoLiver.Jobs.Job
{
    [DisallowConcurrentExecution]
    public class SendHeartBeatJob : BaseJobDescribe, IJob
    {
        private readonly ILogger<SendHeartBeatJob> _logger;
        private readonly IBilibiliAccountApiService _accountService;

        public SendHeartBeatJob(ILogger<SendHeartBeatJob> logger
            , IBilibiliAccountApiService accountService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await SendHeartBeat();
        }

        private async Task SendHeartBeat()
        {
            try
            {
                _logger.LogInformation("发送心跳请求");
                //心跳
                await _accountService.HeartBeat();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"心跳定时任务执行失败，{ex.Message}");
            }
        }

        #region Job info

        private JobMetadata _jobMetadata = null;

        protected override JobMetadata JobMetadata
        {
            get
            {
                if (_jobMetadata != null)
                {
                    return _jobMetadata;
                }
                _jobMetadata = new JobMetadata()
                {
                    Id = 2,
                    Name = $"{this.GetType().Name}Service",
                    IntervalTime = 60,
                    StartTime = DateTime.Now.AddSeconds(60)
                };
                return _jobMetadata;
            }
        }

        #endregion

    }
}
