using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using BilibiliAutoLiver.Services;
using BilibiliAutoLiver.Services.Interface;
using FFMpegCore;
using Microsoft.Extensions.DependencyInjection;

namespace BilibiliAutoLiver.DependencyInjection
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
            string defaultBinPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "runtimes", GetProcessArchitecturePath(), "bin");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string path = Path.Combine(defaultBinPath, "ffmpeg.exe");
                if (File.Exists(path))
                {
                    return defaultBinPath;
                }
                string allPathEnvs = Environment.GetEnvironmentVariable("Path");
                if (!string.IsNullOrWhiteSpace(allPathEnvs))
                {
                    string[] allPath = allPathEnvs.Split(';', StringSplitOptions.RemoveEmptyEntries);
                    if (allPath != null && allPath.Length > 0)
                    {
                        foreach (var pathItem in allPath)
                        {
                            string ffmpegPath = Path.Combine(pathItem, "ffmpeg.exe");
                            if (File.Exists(ffmpegPath))
                            {
                                return pathItem;
                            }
                        }
                    }
                }
                throw new FileNotFoundException($"FFMpeg not found, please download ffmpeg to the path {defaultBinPath}.", defaultBinPath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string path = Path.Combine(defaultBinPath, "ffmpeg");
                if (File.Exists(path))
                {
                    SetUnixFileExecutable(path);
                    return defaultBinPath;
                }
                if (File.Exists("/usr/bin/ffmpeg"))
                {
                    return "/usr/bin";
                }
                throw new FileNotFoundException("FFMpeg not found, please install ffmpeg at first. In a Debian OS, you can use 'apt install ffmpeg' to install it.");
            }
            else
            {
                throw new PlatformNotSupportedException($"Unsupported system type: {RuntimeInformation.OSDescription}");
            }
        }

        private static void SetUnixFileExecutable(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (!File.Exists(path))
                {
                    return;
                }
                UnixFileMode fileMode = File.GetUnixFileMode(path);
                if ((UnixFileMode.UserExecute & fileMode) == UnixFileMode.UserExecute)
                {
                    return;
                }
                File.SetUnixFileMode(path, fileMode | UnixFileMode.UserExecute);
            }
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
