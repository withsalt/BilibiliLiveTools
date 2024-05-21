using System;
using BilibiliAutoLiver.Jobs.Job;
using BilibiliAutoLiver.Jobs.Scheduler;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.AspNetCore;

namespace BilibiliAutoLiver.DependencyInjection
{
    public static class RegisteQuartz
    {
        public static IServiceCollection AddQuartz(this IServiceCollection services)
        {
            services.Configure<QuartzOptions>(options =>
            {
                options.Scheduling.IgnoreDuplicates = false; // default: false
                options.Scheduling.OverWriteExistingData = true; // default: true
            });

            services.AddQuartz(q =>
            {
                q.SchedulerId = $"BilibiliAutoLiverScheduler";
                q.InterruptJobsOnShutdown = true;
                q.UseInMemoryStore();
                q.UseDefaultThreadPool(tp =>
                {
                    tp.MaxConcurrency = 3;
                });
                // auto-interrupt long-running job
                q.UseJobAutoInterrupt(options =>
                {
                    // this is the default
                    options.DefaultMaxRunTime = TimeSpan.FromMinutes(1);
                });
                // convert time zones using converter that can handle Windows/Linux differences
                q.UseTimeZoneConverter();
            });
            //注册JobScheduler
            services.AddSingleton<IJobSchedulerService, JobSchedulerService>();
            //add base job
            services.AddTransient<RefreshCookieJob>();
            services.AddTransient<SendHeartBeatJob>();
            // Quartz.Extensions.Hosting allows you to fire background service that handles scheduler lifecycle
            services.AddQuartzServer(options =>
            {
                // when shutting down we want jobs to complete gracefully
                options.WaitForJobsToComplete = true;
            });

            services.AddHostedService<QuartzJobHostedService>();

            return services;
        }

    }
}
