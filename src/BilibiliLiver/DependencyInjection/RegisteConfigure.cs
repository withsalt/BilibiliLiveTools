using BilibiliLiver.Config.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BilibiliLiver.DependencyInjection
{
    public static class RegisteConfigure
    {
        public static IServiceCollection ConfigureSettings(this IServiceCollection services, HostBuilderContext hostContext)
        {
            services.Configure<LiveSetting>(hostContext.Configuration.GetSection(LiveSetting.Position));
            return services;
        }
    }
}
