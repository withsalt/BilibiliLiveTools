using BilibiliLiver.Config;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLiver
{
    class App
    {
        private readonly ILogger<App> _logger;
        private readonly ConfigManager _config;

        public App(ILogger<App> logger
            , ConfigManager config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task Run(params string[] args)
        {
            while (true)
            {
                await Task.Delay(1500);

                _logger.LogInformation($"Account: {_config.UserSetting.Account}");
            }
        }
    }
}
