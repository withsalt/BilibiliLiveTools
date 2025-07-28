using System;
using System.Net;
using System.Net.Http;
using Bilibili.AspNetCore.Apis.Constants;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models;
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
        public static IServiceCollection AddBilibiliApis(this IServiceCollection services, Action<BilibiliAppKey> configure, bool withDefaultCookieProvider = true)
        {
            services.Configure(configure);

            //锁
            services.AddSingleton<ILocalLockService, LocalLockService>();

            //HttpClient
            services.AddHttpClient("BilibiliRequestClient", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(60);

                client.DefaultRequestHeaders.Add("accept", "*/*");
                client.DefaultRequestHeaders.Add("accept-language", "zh-CN,zh;q=0.9");
                client.DefaultRequestHeaders.Add("origin", "https://www.bilibili.com");
                client.DefaultRequestHeaders.Add("referer", "https://www.bilibili.com/");
                client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Chromium\";v=\"124\", \"Google Chrome\";v=\"124\", \"Not-A.Brand\";v=\"99\"");
                client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                client.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
                client.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
                client.DefaultRequestHeaders.Add("sec-fetch-site", "same-site");
                client.DefaultRequestHeaders.Add("user-agent", GlobalConfigConstant.USER_AGENT);
                client.DefaultRequestHeaders.Add("cache-control", "no-cache");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
            });

            services.AddTransient<IHttpClientService, HttpClientService>();

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
