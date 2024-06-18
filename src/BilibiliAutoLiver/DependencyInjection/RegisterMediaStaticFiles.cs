using System.IO;
using BilibiliAutoLiver.Config;
using BilibiliAutoLiver.Models.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;


namespace BilibiliAutoLiver.DependencyInjection
{
    public static class RegisterMediaStaticFiles
    {
        public static IApplicationBuilder UseMediaStaticFiles(this WebApplication app)
        {
            AppSettings appSettings = app.Services.GetService<IOptions<AppSettings>>().Value;
            string mediaDirectory = Path.Combine(appSettings.DataDirectory, GlobalConfigConstant.DefaultMediaDirectory);
            if (!Directory.Exists(mediaDirectory))
            {
                Directory.CreateDirectory(mediaDirectory);
            }
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(mediaDirectory),
                RequestPath = new PathString("/" + GlobalConfigConstant.DefaultMediaDirectory),
            });
            return app;
        }
    }
}
