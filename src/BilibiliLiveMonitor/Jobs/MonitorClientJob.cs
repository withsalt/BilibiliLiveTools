using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BilibiliLiveMonitor.Configs;
using BilibiliLiveMonitor.Models;
using BilibiliLiveMonitor.Services;
using BilibiliLiveMonitor.Utils;
using BilibiliLiveMonitor.Utils.Json;

namespace BilibiliLiveMonitor.Jobs
{
    [DisallowConcurrentExecution]
    public class MonitorClientJob : IJob
    {
        private readonly ILogger<MonitorClientJob> _logger;
        private readonly IMemoryCache _cache;
        private readonly IEmailNoticeService _email;
        private readonly AppSettingsNode _appSettings;

        private const string UPSAPIBASE = "{0}/ViewPower/workstatus/reqMonitorData{1}";

        /// <summary>
        /// 开机时间 分钟
        /// </summary>
        private int _startUpTime
        {
            get
            {
                double val = Environment.TickCount / 1000.0 / 60.0;
                if (val < 0) val = -val;
                return (int)val;
            }
        }

        public MonitorClientJob(ILogger<MonitorClientJob> logger
            , IMemoryCache cache
            , IEmailNoticeService email
            , IOptions<AppSettingsNode> appSettingsOptions
            , IOptions<ShutdownSettingsNode> shutdownSettingsOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _email = email ?? throw new ArgumentNullException(nameof(email));
            _appSettings = appSettingsOptions.Value ?? throw new ArgumentNullException(nameof(appSettingsOptions));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 发送通知
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        private async Task SendEmailNotice(string reason)
        {
            try
            {
                var result = await _email.Send("PVE关机告警", reason);
                if (result.Item1 != SendStatus.Success)
                {
                    throw new Exception(result.Item2);
                }
                _logger.LogInformation("通知邮件已发送！");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"发送邮件通知失败，错误：{ex.Message}");
            }
        }
    }
}
