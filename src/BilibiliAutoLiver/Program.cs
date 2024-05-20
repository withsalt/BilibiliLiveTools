using System;
using System.Reflection;
using Bilibili.AspNetCore.Apis.DependencyInjection;
using BilibiliAutoLiver.Config;
using BilibiliAutoLiver.DependencyInjection;
using BilibiliAutoLiver.Services;
using BilibiliAutoLiver.Services.Interface;
using BilibiliAutoLiver.Plugin.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System.Threading;
using BilibiliAutoLiver.Jobs;
using Microsoft.AspNetCore.Authentication.Cookies;
using BilibiliAutoLiver.Utils;

namespace BilibiliAutoLiver
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = $"Bilibili无人值守直播工具 v{VersionHelper.GetVersion()} By withsalt(https://github.com/withsalt)";

            var builder = WebApplication.CreateBuilder(args);

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

            builder.Services.AddSingleton<IPushStreamServiceV1, PushStreamServiceV1>();
            builder.Services.AddSingleton<IPushStreamServiceV2, PushStreamServiceV2>();
            builder.Services.AddSingleton<IPushStreamProxyService, PushStreamProxyService>();
            builder.Services.AddTransient<IStartupService, StartupService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(GlobalConfigConstant.DefaultOriginsName, policy =>
                {
                    policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
                });
            });

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromDays(3650*10);
                    options.SlidingExpiration = true;
                    options.AccessDeniedPath = "/Account/Login";
                    options.LogoutPath = "/Account/Login";
                });

            // Add services to the container.
            builder.Services.AddControllersWithViews()
                .AddRazorRuntimeCompilation(); ;

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //跨域
            app.UseCors(GlobalConfigConstant.DefaultOriginsName);

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Lifetime.ApplicationStarted.Register((obj, token)
                => app.Services.GetRequiredService<IStartupService>().Start(token), null);

            app.Run();
        }
    }
}
