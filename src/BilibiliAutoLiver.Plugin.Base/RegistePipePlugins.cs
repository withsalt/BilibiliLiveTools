using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BilibiliAutoLiver.Plugin.Base.Loader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Plugin.Base
{
    public static class RegistePipePlugins
    {
        public static IServiceCollection AddPipePlugins(this IServiceCollection services)
        {
            var loaders = new List<PluginLoader>();
            var container = new PipeContainer();
            services.AddSingleton<IPipeContainer>(container);

            using ServiceProvider provider = services.BuildServiceProvider();
            ILogger<IPipeProcess> logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger<IPipeProcess>();

            string[]? dllNames = Directory.GetFiles(AppContext.BaseDirectory, "BilibiliAutoLiver.Plugin.*.dll", SearchOption.AllDirectories)
                ?.Where(p => !Path.GetFileName(p).Equals("BilibiliAutoLiver.Plugin.Base.dll", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (dllNames == null || dllNames.Length == 0)
            {
                logger.LogInformation("没有插件需要加载。");
                return services;
            }

            HashSet<string> exists = new HashSet<string>();
            foreach (var fileName in dllNames)
            {
                try
                {
                    var loader = PluginLoader.CreateFromAssemblyFile(fileName, sharedTypes: [typeof(IPipeProcess)]);
                    IEnumerable<Type>? types = loader.LoadDefaultAssembly().GetTypes().Where(t => typeof(IPipeProcess).IsAssignableFrom(t) && !t.IsAbstract);
                    if (types?.Any() != true) continue;

                    foreach (var pluginType in types)
                    {
                        object? instance = Activator.CreateInstance(pluginType);
                        if (instance == null || string.IsNullOrEmpty(pluginType.FullName)) continue;
                        if (exists.Contains(pluginType.FullName))
                        {
                            continue;
                        }
                        IPipeProcess pipeProcess = (IPipeProcess)instance;
                        if (string.IsNullOrWhiteSpace(pipeProcess.Name) || pipeProcess.Index <= 0)
                        {
                            continue;
                        }
                        char[] invalidChars = Path.GetInvalidPathChars();
                        if (pipeProcess.Name.IndexOfAny(invalidChars) >= 0)
                        {
                            throw new Exception("加载插件【{pipeProcess.Name}】失败，插件名称包含特殊字符。");
                        }
                        logger.LogInformation($"加载插件{pipeProcess.Name}（{fileName}）");
                        container.Add(pipeProcess);
                        exists.Add(pluginType.FullName);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"加载插件{fileName}失败，{ex.Message}");
                    continue;
                }
            }
            return services;
        }
    }
}
