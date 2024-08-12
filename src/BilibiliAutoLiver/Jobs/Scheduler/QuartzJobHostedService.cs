using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Jobs.Scheduler
{
    public class QuartzJobHostedService : IHostedService
    {
        private readonly ILogger<QuartzJobHostedService> _logger;
        private readonly IServiceProvider _provider;
        private readonly IdleBus<IFreeSql> _ib;

        public QuartzJobHostedService(ILogger<QuartzJobHostedService> logger
            , IServiceProvider provider
            , IdleBus<IFreeSql> ib)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _ib = ib ?? throw new ArgumentNullException(nameof(ib));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _provider.GetRequiredService<IJobSchedulerService>()
                .Stop(cancellationToken);

            //释放IdleBus，就不单独写个Host去释放了
            //为什么没有在容器销毁的时候自动释放呢？
            if (_ib != null)
            {
                _ib.Dispose();
            }
        }
    }
}
