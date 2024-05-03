using BilibiliAutoLiver.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BilibiliAutoLiver.DependencyInjection
{
    public static class RegisteConfigure
    {
        public static IServiceCollection ConfigureSettings(this IServiceCollection services, IHostApplicationBuilder builder)
        {
            services.Configure<LiveSettings>(builder.Configuration.GetSection(LiveSettings.Position));
            return services;
        }
    }
}
