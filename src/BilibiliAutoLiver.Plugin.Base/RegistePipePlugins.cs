using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BilibiliAutoLiver.Plugin.Base.Loader;
using Microsoft.Extensions.DependencyInjection;

namespace BilibiliAutoLiver.Plugin.Base
{
    public static class RegistePipePlugins
    {
        public static IServiceCollection AddPipePlugins(this IServiceCollection services)
        {
            var loaders = new List<PluginLoader>();
            var container = new PipeContainer();

            string[]? dllNames = Directory.GetFiles(AppContext.BaseDirectory, "BilibiliAutoLiver.Plugin.*.dll", SearchOption.AllDirectories)
                ?.Where(p => !Path.GetFileName(p).Equals("BilibiliAutoLiver.Plugin.Base.dll", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (dllNames == null || dllNames.Length == 0)
                return services;

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
                        if (instance == null) continue;
                        container.Add((IPipeProcess)instance);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"加载模块{fileName}失败，{ex.Message}");
                    continue;
                }
            }

            services.AddSingleton<IPipeContainer>(container);
            return services;
        }
    }
}
