using BilibiliLiveMonitor.Configs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BilibiliLiveMonitor.DependencyInjection
{
    public static class RegisteConfigure
    {
        public static IServiceCollection ConfigureSettings(this IServiceCollection services, HostBuilderContext hostContext)
        {
            //配置文件
            services.Configure<AppSettingsNode>(hostContext.Configuration.GetSection(AppSettingsNode.Position));
            services.Configure<LiveSettingsNode>(hostContext.Configuration.GetSection(LiveSettingsNode.Position));
            return services;
        }
    }
}
