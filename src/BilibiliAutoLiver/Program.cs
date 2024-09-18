using System;
using System.Threading;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.DependencyInjection;
using BilibiliAutoLiver.Config;
using BilibiliAutoLiver.DependencyInjection;
using BilibiliAutoLiver.Plugin.Base;
using BilibiliAutoLiver.Services;
using BilibiliAutoLiver.Services.FFMpeg.Services.PushService;
using BilibiliAutoLiver.Services.Interface;
using BilibiliAutoLiver.Utils;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog.Web;

namespace BilibiliAutoLiver
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.Title = $"哔哩哔哩无人值守直播工具 v{VersionHelper.GetVersion()} By withsalt(https://github.com/withsalt)";

            var builder = WebApplication.CreateBuilder(args);

            //Add NLog
            builder.Logging.ClearProviders();
            builder.Logging.AddNLogWeb();

            //配置初始化
            builder.Services.ConfigureSettings(builder);

            //Db
            builder.Services.AddDatabase();
            builder.Services.AddRepository();

            //缓存
            builder.Services.AddMemoryCache();
            //添加Bilibili相关的服务
            builder.Services.AddBilibiliApis(false);
            //定时任务
            builder.Services.AddQuartz();
            //FFMpeg
            builder.Services.AddFFmpegService();
            //插件
            builder.Services.AddPipePlugins();
            //邮件服务
            builder.Services.AddTransient<IEmailNoticeService, EmailNoticeService>();

            builder.Services.AddSingleton<INormalPushStreamService, NormalPushStreamService>();
            builder.Services.AddSingleton<IAdvancePushStreamService, AdvancePushStreamService>();
            builder.Services.AddSingleton<IPushStreamProxyService, PushStreamProxyService>();
            builder.Services.AddTransient<IStartupService, StartupService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(GlobalConfigConstant.DEFAULT_ORIGINS_NAME, policy =>
                {
                    policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
                });
            });

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromDays(3650 * 10);
                    options.SlidingExpiration = true;
                    options.AccessDeniedPath = "/Account/Login";
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.ReturnUrlParameter = "";
                });

            //使用一个默认参数
            var urls = builder.Configuration["ASPNETCORE_URLS"] ?? builder.Configuration["urls"];
            if (string.IsNullOrWhiteSpace(urls))
            {
                builder.WebHost.UseUrls("http://*:18686");
            }

#if DEBUG
            builder.Services.AddControllersWithViews()
                .AddNewtonsoftJson(options => {
                    options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                    options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                }).AddRazorRuntimeCompilation();
#else
            builder.Services.AddControllersWithViews()
                .AddNewtonsoftJson(options => {
                    options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                    options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });
#endif

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //跨域
            app.UseCors(GlobalConfigConstant.DEFAULT_ORIGINS_NAME);

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseMediaStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            //初始化数据库
            app.InitializeDatabase();

            app.Lifetime.ApplicationStarted.Register((obj, token)
                => Task.Run(() => app.Services.GetRequiredService<IStartupService>().Start(token), CancellationToken.None), null);

            await app.RunAsync();
        }
    }
}
