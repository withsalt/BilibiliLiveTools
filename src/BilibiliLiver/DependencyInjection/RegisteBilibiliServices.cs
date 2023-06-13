using BilibiliLiveCommon.Services;
using BilibiliLiveCommon.Services.Interface;
using BilibiliLiver.Services;
using BilibiliLiver.Services.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace BilibiliLiver.DependencyInjection
{
    public static class RegisteBilibiliServices
    {
        public static IServiceCollection AddBilibiliServices(this IServiceCollection services)
        {
            //账号
            services.AddTransient<IBilibiliAccountService, BilibiliAccountService>();
            //Cookie模块
            services.AddTransient<IBilibiliCookieService, BilibiliCookieService>();
            //Http请求相关
            services.AddTransient<IHttpClientService, HttpClientService>();
            //直播的API
            services.AddTransient<IBilibiliLiveApiService, BilibiliLiveApiService>();
            //推流相关
            services.AddTransient<IPushStreamService, PushStreamService>();

            return services;
        }
    }
}
