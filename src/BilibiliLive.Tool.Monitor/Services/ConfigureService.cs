using System;
using System.Threading;
using System.Threading.Tasks;
using BilibiliLive.Tool.Monitor.Services.Interface;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BilibiliLive.Tool.Monitor.Services
{
    public class ConfigureService : IHostedService
    {
        private readonly ILogger<ConfigureService> _logger;
        private readonly IJobSchedulerService _schedulerService;

        public ConfigureService(ILogger<ConfigureService> logger
            , IJobSchedulerService schedulerService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //start work task
            await _schedulerService.Start();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
