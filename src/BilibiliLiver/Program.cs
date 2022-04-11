using BilibiliLiver.DependencyInjection;
using BilibiliLiver.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;

namespace BilibiliLiver
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.Title = $"Bilibili直播工具 v{Assembly.GetExecutingAssembly().GetName().Version} By withsalt(https://github.com/withsalt)";

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    //配置初始化
                    services.ConfigureSettings(hostContext);
                    //缓存
                    services.AddMemoryCache();
                    //添加Bilibili相关的服务
                    services.AddBilibiliServices();
                    //配置并启动服务
                    services.AddHostedService<ConfigureService>();
                });
    }
}
