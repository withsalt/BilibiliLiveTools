﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using BilibiliLiveMonitor.Services;
using BilibiliLiveMonitor.DependencyInjection;

namespace BilibiliLiveMonitor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    //移除已经注册的其他日志处理程序
                    logging.ClearProviders();
                    //设置最小的日志级别
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
                .UseNLog()
                .ConfigureServices((hostContext, services) =>
                {
                    //配置
                    services.ConfigureSettings(hostContext);
                    //缓存
                    services.AddMemoryCache();
                    //Bilibili相关API
                    services.AddBilibiliServices();
                    //定时任务
                    services.AddQuartz();
                    //启动服务
                    services.AddHostedService<ConfigureService>();
                });
    }
}