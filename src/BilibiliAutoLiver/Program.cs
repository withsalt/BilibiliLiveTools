using System.Reflection;
using BilibiliAutoLiver.Services;
using BilibiliLiveCommon.Config;
using NLog.Web;

namespace BilibiliAutoLiver
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = $"Bilibili无人值守直播工具 v{Assembly.GetExecutingAssembly().GetName().Version} By withsalt(https://github.com/withsalt)";

            var builder = WebApplication.CreateBuilder(args);

            //Add NLog
            builder.Logging.ClearProviders();
            builder.Logging.AddNLogWeb();

            builder.Services.AddSingleton<IStartupService, StartupService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(GlobalConfigConstant.DefaultOriginsName, policy =>
                {
                    policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
                });
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews();

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

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            IHostApplicationLifetime lifetime = app.Lifetime;
            IStartupService startService = app.Services.GetRequiredService<IStartupService>();

            lifetime.ApplicationStarted.Register(() =>
            {
                startService.Start();
            });
            app.Run();
        }
    }
}
