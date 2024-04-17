using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using FFMpegCore;
using Microsoft.Extensions.DependencyInjection;

namespace BilibiliLiveCommon.FFMpeg
{
    public static class RegisterFFmpegService
    {
        static RegisterFFmpegService()
        {
            GlobalFFOptions.Configure(options =>
            {
                options.BinaryFolder = GetBinaryFolder();
                options.TemporaryFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tmp");
                options.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                options.Encoding = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Encoding.Default : Encoding.UTF8;
                
                if (Path.Exists(options.TemporaryFilesFolder))
                {
                    Directory.CreateDirectory(options.TemporaryFilesFolder);
                }
            });
        }

        public static IServiceCollection AddFFmpegService(this IServiceCollection services)
        {
            services.AddSingleton<IFFMpegService, FFMpegService>();
            return services;
        }

        #region private

        private static string GetBinaryFolder()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "runtimes", GetProcessArchitecturePath());
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return path;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string binPath = Path.Combine(path, "ffmpeg");
                if (File.Exists(binPath))
                {
                    return path;
                }
                return "/usr/bin";
            }
            return null;
        }

        private static string GetProcessArchitecturePath()
        {
            string architecture = string.Empty;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                architecture = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X86 => "win-x86",
                    Architecture.X64 => "win-x64",
                    Architecture.Arm64 => "win-arm64",
                    _ => throw new PlatformNotSupportedException($"Unsupported processor architecture: {RuntimeInformation.ProcessArchitecture}"),
                };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                architecture = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X64 => "linux-x64",
                    Architecture.Arm => "linux-arm",
                    Architecture.Arm64 => "linux-arm64",
#if NET7_0_OR_GREATER
                    Architecture.LoongArch64 => "linux-loongarch64",
#endif
                    _ => throw new PlatformNotSupportedException($"Unsupported processor architecture: {RuntimeInformation.ProcessArchitecture}"),
                };
            }
            else
            {
                throw new PlatformNotSupportedException($"Unsupported system type: {RuntimeInformation.OSDescription}");
            }
            return architecture;
        }

        #endregion
    }
}
