using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models;
using BilibiliAutoLiver.Services.Base;
using BilibiliAutoLiver.Services.FFMpeg.Services.CliBinder;
using BilibiliAutoLiver.Services.Interface;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Services.FFMpeg.Services
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
                binName = "ffmpeg.exe";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                binName = "ffmpeg";
            else
                throw new PlatformNotSupportedException($"Unsupported system type: {RuntimeInformation.OSDescription}");

            string path = Path.Combine(GetBinaryFolder(), binName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("FFMpeg not found, please download or install ffmpeg at firist.", path);
            }
            return path;
        }

        public async Task<List<string>> GetVideoDevices()
        {
            return await GetCliBinder().GetVideoDevices();
        }

        public async Task<List<string>> GetAudioDevices()
        {
            return await GetCliBinder().GetAudioDevices();
        }

        public async Task<List<string>> ListVideoDeviceSupportResolutions(string deviceName)
        {
            return await GetCliBinder().ListVideoDeviceSupportResolutions(deviceName);
        }

        public async Task<LibVersion> GetVersion()
        {
            return await GetCliBinder().GetVersion();
        }

        private IFFMpegCliBinder GetCliBinder()
        {
            IFFMpegCliBinder binder = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                binder = new FFMpegWindowsCliBinder(GetBinaryPath(), GetBinaryFolder());
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                binder = new FFMpegLinuxCliBinder(GetBinaryPath(), GetBinaryFolder());
            else
                throw new PlatformNotSupportedException($"Unsupported system type: {RuntimeInformation.OSDescription}");
            return binder;
        }
    }
}
