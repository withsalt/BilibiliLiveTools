using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using BilibiliLiveMonitor.Configs;
using BilibiliLiveMonitor.Services;

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
                    //配置文件
                    services.Configure<AppSettingsNode>(hostContext.Configuration.GetSection(AppSettingsNode.Position));
                    services.Configure<ShutdownSettingsNode>(hostContext.Configuration.GetSection(ShutdownSettingsNode.Position));
                    //Http客户端
                    services.AddHttpClient();
                    //缓存
                    services.AddMemoryCache();
                    //定时任务
                    services.AddQuartz();
                    //配置并启动服务
                    services.AddTransient<IEmailNoticeService, EmailNoticeService>();
                    services.AddTransient<IShutdownService, ShutdownService>();
                    services.AddHostedService<ConfigureService>();
                });
    }
}