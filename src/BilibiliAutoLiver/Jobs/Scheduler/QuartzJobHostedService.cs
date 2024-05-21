using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BilibiliAutoLiver.Jobs.Scheduler
{
    public class QuartzJobHostedService : IHostedService
    {

        private readonly ILogger<QuartzJobHostedService> _logger;
        private readonly IServiceProvider _provider;

        public QuartzJobHostedService(ILogger<QuartzJobHostedService> logger
            , IServiceProvider provider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _provider.GetRequiredService<IJobSchedulerService>()
                .StopAsync(cancellationToken);
        }
    }
}
