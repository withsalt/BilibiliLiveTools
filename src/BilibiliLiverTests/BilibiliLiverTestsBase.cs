using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System;
using BilibiliAutoLiver.DependencyInjection;
using Bilibili.AspNetCore.Apis.DependencyInjection;

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

            builder.Build();

            this.ServiceProvider = builder.Services.BuildServiceProvider();
        }
    }
}
