using System;
using Bilibili.AspNetCore.Apis.DependencyInjection;
using BilibiliAutoLiver.DependencyInjection;
using BilibiliAutoLiver.Plugin.Base;
using BilibiliAutoLiver.Services;
using BilibiliAutoLiver.Services.Interface;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace BilibiliLiverTests
{
    public class BilibiliLiverTestsBase
    {
        public IServiceProvider ServiceProvider { get; set; }

        public BilibiliLiverTestsBase()
        {
            var builder = WebApplication.CreateBuilder();

            //Add NLog
            builder.Logging.ClearProviders();
            builder.Logging.AddNLogWeb();

            //配置初始化
            builder.Services.ConfigureSettings(builder);
            //缓存
            builder.Services.AddMemoryCache();
            //添加Bilibili相关的服务
            builder.Services.AddBilibiliApis();
            //定时任务
            builder.Services.AddQuartz();
            //FFMpeg
            builder.Services.AddFFmpegService();
            //插件
            builder.Services.AddPipePlugins();
            //邮件服务
            builder.Services.AddTransient<IEmailNoticeService, EmailNoticeService>();

            builder.Services.AddSingleton<IPushStreamServiceV1, PushStreamServiceV1>();
            builder.Services.AddSingleton<IPushStreamServiceV2, PushStreamServiceV2>();
            builder.Services.AddSingleton<IPushStreamProxyService, PushStreamProxyService>();


            //Db
            builder.Services.AddDatabase();
            builder.Services.AddRepository();

            builder.Build();

            this.ServiceProvider = builder.Services.BuildServiceProvider();
        }
    }
}
