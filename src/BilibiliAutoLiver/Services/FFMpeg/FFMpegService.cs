using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BilibiliLiveCommon.Model;
using CliWrap;
using CliWrap.Buffered;
using FFMpegCore;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Services.FFMpeg
{
    public class FFMpegService : BaseFFPlayService, IFFMpegService
    {
        private readonly ILogger<FFMpegService> _logger;

        public FFMpegService(ILogger<FFMpegService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Snapshot(string filePath, string outPath, int width, int height, int cutTime)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(nameof(filePath));
            }
            bool snapshotResult = await FFMpegCore.FFMpeg.SnapshotAsync(filePath, outPath, new Size(width, height), TimeSpan.FromSeconds(cutTime));
            if (!snapshotResult)
            {
                throw new Exception($"Snapshot in {cutTime} from video '{filePath}' failed.");
            }
            if (!File.Exists(outPath))
            {
                throw new FileNotFoundException("The snapshot output file not exist.", outPath);
            }
            return true;
        }

        public string GetBinaryPath()
        {
            string binName;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                binName = "ffmpeg.exe";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                binName = "ffmpeg";
            }
            else
            {
                throw new PlatformNotSupportedException($"Unsupported system type: {RuntimeInformation.OSDescription}");
            }

            string path = Path.Combine(GetBinaryFolder(), binName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("ffmpeg not found, please download ffmpeg in target os path.", path);
            }
            return path;
        }

        public async Task<LibVersion> GetVersion()
        {
            LibVersion libVersion = new LibVersion()
            {
                Status = false
            };
            try
            {
                var result = await Cli.Wrap(GetBinaryPath())
                    .WithArguments("-version")
                    .WithWorkingDirectory(GetBinaryFolder())
                    .WithValidation(CommandResultValidation.None)
                    .ExecuteBufferedAsync();
                libVersion.Status = true;
                string output = result.StandardOutput;
                if (result.ExitCode != 0)
                {
                    if (result.StandardError?.StartsWith("ffmpeg version ", StringComparison.Ordinal) == true)
                    {
                        output = result.StandardError;
                    }
                    else
                    {
                        return libVersion;
                    }
                }

                if (string.IsNullOrEmpty(output))
                {
                    return libVersion;
                }
                if (output.StartsWith("ffmpeg version ", StringComparison.Ordinal))
                {
                    int startIndex = "ffmpeg version ".Length;
                    var lastIndex = output.IndexOf(' ', startIndex);
                    if (lastIndex > startIndex + 1)
                    {
                        libVersion.Version = output.Substring(startIndex, lastIndex - startIndex + 1);
                    }
                }
                return libVersion;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Try get ffmpeg version failed. {ex.Message}");
                return libVersion;
            }
        }
    }
}
