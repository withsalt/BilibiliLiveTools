using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Providers;
using Bilibili.AspNetCore.Apis.Services;
using Bilibili.AspNetCore.Apis.Services.Lock;
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
            //锁
            services.AddSingleton<ILocalLockService, LocalLockService>();

            if (withDefaultCookieProvider)
            {
                services.AddSingleton<IBilibiliCookieRepositoryProvider, BilibiliCookieFileRepositoryProvider>();
            }

            //Cookie模块
            services.AddTransient<IBilibiliCookieService, BilibiliCookieService>();
            //账号
            services.AddTransient<IBilibiliAccountApiService, BilibiliAccountApiService>();
            //直播的API
            services.AddTransient<IBilibiliLiveApiService, BilibiliLiveApiService>();

            return services;
        }
    }
}
