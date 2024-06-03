using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Providers;
using Bilibili.AspNetCore.Apis.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Bilibili.AspNetCore.Apis.DependencyInjection
{
    public static class RegisteBilibiliApis
    {
        /// <summary>
        /// 注册Bilibili相关服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="withDefaultCookieProvider">使用使用默认的仓储</param>
        /// <returns></returns>
        public static IServiceCollection AddBilibiliApis(this IServiceCollection services, bool withDefaultCookieProvider = true)
        {
            if (withDefaultCookieProvider)
            {
                services.AddSingleton<IBilibiliCookieRepositoryProvider, BilibiliCookieFileRepositoryProvider>();
            }
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
