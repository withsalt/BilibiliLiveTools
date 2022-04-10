using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BilibiliLiver.Services
{
    public class ConfigureService : IHostedService
    {
        private readonly ILogger<ConfigureService> _logger;
        private readonly IConfiguration _configuration;

        public ConfigureService(ILogger<ConfigureService> logger
            , IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
