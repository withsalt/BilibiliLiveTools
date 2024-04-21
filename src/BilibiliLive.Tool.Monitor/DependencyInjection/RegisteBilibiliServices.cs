using Microsoft.Extensions.DependencyInjection;
using BilibiliAutoLiver.Services;
using BilibiliAutoLiver.Services.Interface;
using BilibiliLive.Tool.Monitor.Services.Interface;
using BilibiliLive.Tool.Monitor.Services;

namespace BilibiliLive.Tool.Monitor.DependencyInjection
{
    public static class RegisteBilibiliServices
    {
        public static IServiceCollection AddBilibiliServices(this IServiceCollection services)
        {
            //Cookie模块
            services.AddSingleton<IBilibiliCookieService, BilibiliCookieService>();
            //Http请求相关
            services.AddTransient<IHttpClientService, HttpClientService>();
            //直播的API
            services.AddTransient<IBilibiliLiveApiService, BilibiliLiveApiService>();
            //邮件
            services.AddTransient<IEmailNoticeService, EmailNoticeService>();
            return services;
        }
    }
}
