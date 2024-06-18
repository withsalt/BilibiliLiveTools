using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Bilibili.AspNetCore.Apis.Providers;
using BilibiliAutoLiver.Config;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Models.Settings;
using BilibiliAutoLiver.Repository;
using BilibiliAutoLiver.Repository.Base;
using BilibiliAutoLiver.Repository.Interface;
using BilibiliAutoLiver.Services;
using FreeSql;
using FreeSql.DataAnnotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BilibiliAutoLiver.DependencyInjection
{
    public static class RegisterDatabase
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services)
        {
            using (var provider = services.BuildServiceProvider())
            {
                ILogger<IFreeSql> logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger<IFreeSql>();
                AppSettings appSettings = provider.GetRequiredService<IOptions<AppSettings>>().Value;

                IdleBus<IFreeSql> ib = new IdleBus<IFreeSql>(TimeSpan.FromMinutes(60));
                //创建IFreeSql对象
                var registerResult = ib.TryRegister("Sqlite", () =>
                {
                    //create builder
                    FreeSqlBuilder fsqlBuilder = new FreeSqlBuilder()
                        .UseConnectionString(FreeSql.DataType.Sqlite, appSettings.DbConnectionString)
                        .UseAutoSyncStructure(true);
                    //如果数据库不存在，那么自动创建数据库
                    CreateDatabaseIfNotExists(fsqlBuilder, appSettings.DbConnectionString);
                    //必须在构建之前创建数据库出来
                    IFreeSql fsql = fsqlBuilder.Build();
                    //sql执行日志
                    fsql.Aop.CurdAfter += (s, e) =>
                    {
                        logger.LogDebug($"Sqlite(thread-{Thread.CurrentThread.ManagedThreadId}):\n  Namespace: {e.EntityType.FullName} \nElapsedTime: {e.ElapsedMilliseconds}ms \n        SQL: {e.Sql}");
                    };
                    return fsql;
                });
                if (!registerResult)
                {
                    throw new Exception($"Register database sqlite failed.");
                }
                //注入
                services.AddScoped<BaseUnitOfWorkManager>();
                //注入IdleBus<IFreeSql>
                services.TryAddSingleton(ib);
                return services;
            }
        }

        public static IServiceCollection AddRepository(this IServiceCollection services)
        {
            services.AddScoped<IPushSettingRepository, PushSettingRepository>();
            services.AddScoped<ILiveSettingRepository, LiveSettingRepository>();
            services.AddScoped<IMonitorSettingRepository, MonitorSettingRepository>();
            services.AddScoped<ICookieSettingRepository, CookieSettingRepository>();

            //Cookie仓储提供器
            services.AddSingleton<IBilibiliCookieRepositoryProvider, BilibiliCookieDbRepositoryProvider>();
            return services;
        }

        /// <summary>
        /// 初始化数据库
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder InitializeDatabase(this IApplicationBuilder app)
        {
            IdleBus<IFreeSql> ib = app.ApplicationServices.GetRequiredService<IdleBus<IFreeSql>>();
            if (ib == null)
            {
                throw new Exception("Get idlebus service failed.");
            }
            IWebHostEnvironment env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
            ILoggerFactory loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger(nameof(RegisterDatabase));

            IFreeSql db = ib.Get("Sqlite");
            if (db == null)
            {
                throw new Exception($"Can not get database sqlite from IdleBus.");
            }
            logger.LogInformation($"For sqlite, automatic sync database structure is turned on, start seeding database...");
            //同步表结构
            SyncStructure(db);
            //写入种子数据
            db.Transaction(() =>
            {
                var pushSetting = new PushSetting()
                {
                    Model = ConfigModel.Normal,
                    FFmpegCommand = "//在Windows环境下调用ffmpeg的命令，使用USB摄像头，设备为HD Pro Webcam C920。\r\n//Windows:\r\nffmpeg -f dshow -video_size 1280x720 -i video=\"HD Pro Webcam C920\" -vcodec libx264 -pix_fmt yuv420p -r 30 -s 1280*720 -g 60 -b:v 5000k -an -preset:v ultrafast -tune:v zerolatency -f flv [[URL]]\r\n\r\n//Linux:\r\n//ffmpeg -thread_queue_size 1024 -f v4l2 -s 1280*720 -input_format mjpeg -i \"/dev/video0\" -stream_loop -1 -i \"Content/demo_music.m4a\" -vcodec h264_omx -pix_fmt yuv420p -r 30 -s 1280*720 -g 60 -b:v 10M -bufsize 10M -acodec aac -ac 2 -ar 44100 -ab 128k -f flv [[URL]]\r\n",
                    IsAutoRetry = true,
                    RetryInterval = 30,
                    CreatedTime = DateTime.UtcNow,
                    CreatedUserId = GlobalConfigConstant.SYS_USERID,
                    IsUpdate = false,
                };
                db.Insert(pushSetting).ExecuteAffrows();
            });

            return app;
        }

        /// <summary>
        /// 同步表结构
        /// </summary>
        /// <param name="fsql"></param>
        private static void SyncStructure(IFreeSql fsql)
        {
            if (!fsql.CodeFirst.IsAutoSyncStructure)
            {
                return;
            }
            List<Type> tableAssembies = new List<Type>();
            var entities = GetExportedTypesByInterface(typeof(IEntity));
            foreach (Type type in entities)
            {
                if (type.GetCustomAttribute<TableAttribute>() != null
                    && type.BaseType != null
                    && (type.BaseType == typeof(IBaseEntity)
                    || type.BaseType == typeof(IBaseEntity<long>)
                    || type.BaseType == typeof(IBaseEntity<int>)))
                {
                    tableAssembies.Add(type);
                }
            }
            if (tableAssembies.Count == 0)
            {
                return;
            }
            fsql.CodeFirst.SyncStructure(tableAssembies.ToArray());
        }

        private static void CreateDatabaseIfNotExists(FreeSqlBuilder freeSql, string connectionString)
        {
            SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder(connectionString);
            FileInfo file = new FileInfo(builder.DataSource);
            if (file.Exists)
            {
                return;
            }
            if (!file.Directory.Exists)
            {
                file.Directory.Create();
            }
        }

        private static List<Type> GetExportedTypesByInterface(Type interfaceType, bool allowInterface = false)
        {
            List<Type> result = new List<Type>();
            List<Type> allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(p => p.GetTypes()).ToList();
            foreach (var p in allTypes)
            {
                Type[] interfaceTypes = p.GetInterfaces();
                if (interfaceTypes == null || interfaceTypes.Length == 0)
                {
                    continue;
                }
                if (allowInterface)
                {
                    if (interfaceTypes.Contains(interfaceType))
                    {
                        result.Add(p);
                    }
                }
                else
                {
                    if (interfaceTypes.Contains(interfaceType) && !p.IsInterface && !p.IsAbstract && p.IsClass)
                    {
                        result.Add(p);
                    }
                }
            }
            return result;
        }
    }
}
