using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace BilibiliAutoLiver.Plugin.Base
{
    public static class RegistePipePlugins
    {
        public static IServiceCollection AddPipePlugins(this IServiceCollection services)
        {
            //string? basePath = Path.GetDirectoryName(typeof(IPipeProcess).Assembly.Location);
            //if(basePath == null) basePath = string.Empty;
            //var defaultPluginCatalog = new FolderPluginCatalog(basePath, type =>
            //{
            //    type.Implements<IPipeProcess>();
            //});
            //var pluginFolderCatalog = new FolderPluginCatalog(Path.Combine(basePath, "plugins"), type =>
            //{
            //    type.Implements<IPipeProcess>();
            //});
            //services.AddPluginFramework()
            //    .AddPluginCatalog(defaultPluginCatalog)
            //    .AddPluginCatalog(pluginFolderCatalog)
            //    .AddPluginType<IPipeProcess>();

            //services.AddPluginFramework<IPipeProcess>();

            return services;
        }
    }
}
