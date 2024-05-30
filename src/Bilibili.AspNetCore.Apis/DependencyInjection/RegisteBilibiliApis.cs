using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Bilibili.AspNetCore.Apis.DependencyInjection
{
    public static class RegisteBilibiliApis
    {
        public static IServiceCollection AddBilibiliApis(this IServiceCollection services)
        {
            //Cookie模块
            services.AddSingleton<IBilibiliCookieService, BilibiliCookieService>();
            //账号
            services.AddTransient<IBilibiliAccountApiService, BilibiliAccountApiService>();
            //直播的API
            services.AddTransient<IBilibiliLiveApiService, BilibiliLiveApiService>();

            return services;
        }
    }
}
