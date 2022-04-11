using BilibiliLiver.Services;
using BilibiliLiver.Services.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace BilibiliLiver.DependencyInjection
{
    public static class RegisteBilibiliServices
    {
        public static IServiceCollection AddBilibiliServices(this IServiceCollection services)
        {
            //Cookie模块
            services.AddSingleton<IBilibiliCookieService, BilibiliCookieService>();
            services.AddTransient<IHttpClientService, HttpClientService>();
            services.AddTransient<IAccountService, BilibiliAccountService>();
            services.AddTransient<IBilibiliLiveApiService, BilibiliLiveApiService>();

            //推流相关
            //services.AddTransient<PushStreamService>();

            return services;
        }
    }
}
