using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLiver.Config
{
    public static class ConfigManageExtension
    {
        public static IServiceCollection AddConfig(this IServiceCollection services)
        {
            services.AddSingleton<ConfigManager>();
            return services;
        }


        public static IApplicationBuilder UseConfig(this IApplicationBuilder app)
        {
            try
            {
                ConfigManager config = app.ApplicationServices.GetService<ConfigManager>();
                if (config == null)
                {
                    throw new Exception("Get config manager service failed.");
                }
                config.Load();
                return app;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
