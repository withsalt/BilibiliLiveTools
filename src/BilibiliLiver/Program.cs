using BilibiliLiver.Api;
using BilibiliLiver.Config;
using BilibiliLiver.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLiver
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // 添加配置管理器
                    services.AddConfig();
                    // 添加 app
                    services.AddTransient<PushStreamService>();

                    services.AddTransient<LiveApi>();

                    //配置并启动服务
                    services.AddHostedService<ConfigureService>();
                });
    }
}
