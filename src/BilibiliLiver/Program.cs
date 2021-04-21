using BilibiliLiver.Api;
using BilibiliLiver.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLiver
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //编码注册
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //DI
            IServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            var provider = services.BuildServiceProvider();

            #region 初始化配置

            ConfigManager config = provider.GetRequiredService<ConfigManager>();
            config.Load();

            #endregion

            #region Main App

            var app = provider.GetRequiredService<App>();
            await app.Run(args);

            #endregion
        }

        static void ConfigureServices(IServiceCollection services)
        {
            // 创建 config
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            services.AddSingleton<IConfiguration>(configuration);

            // 配置日志
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });
            // 添加配置管理器
            services.AddConfig();
            // 添加 app
            services.AddTransient<App>();

            #region 注册API

            services.AddTransient<LiveApi>();

            #endregion
        }
    }
}
