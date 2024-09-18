using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Models.FFMpeg;
using BilibiliAutoLiver.Services.Base;
using BilibiliAutoLiver.Services.FFMpeg.Services.CliBinder;
using BilibiliAutoLiver.Services.Interface;
using FFMpegCore.Enums;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Services.FFMpeg.Services
{
    public class FFMpegService : BaseFFPlayService, IFFMpegService
    {
        private readonly ILogger<FFMpegService> _logger;
        private readonly IMemoryCache _cache;

        private List<Codec> _allSupportsVideoCodecs = null;
        private readonly static object _getVideoCodecsLocker = new object();

        public FFMpegService(ILogger<FFMpegService> logger, IMemoryCache cache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
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

        public IReadOnlyList<Codec> GetVideoCodecs()
        {
            return _cache.GetOrCreate("FFMPEG_MACHINE_VIDEO_CODECS", (entry) =>
            {
                var allVideoCodes = FFMpegCore.FFMpeg.GetVideoCodecs();
                if (allVideoCodes?.Any() != true)
                {
                    return new List<Codec>();
                }
                _allSupportsVideoCodecs = allVideoCodes
                    .Where(p => p.Name.Contains("264", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("265", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("hevc", StringComparison.OrdinalIgnoreCase)).ToList();
                return _allSupportsVideoCodecs;
            });
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
            return await _cache.GetOrCreateAsync("FFMPEG_MACHINE_VIDEO_DEVICES", async (entry) =>
            {
                return await GetCliBinder().GetVideoDevices();
            });
        }

        public async Task<List<string>> GetAudioDevices()
        {
            return await _cache.GetOrCreateAsync("FFMPEG_MACHINE_AUDIO_DEVICES", async (entry) =>
            {
                return await GetCliBinder().GetAudioDevices();
            });
        }

        public async Task<List<DeviceResolution>> ListVideoDeviceSupportResolutions(string deviceName)
        {
            return await _cache.GetOrCreateAsync($"FFMPEG_{deviceName}_SUPPORT_RESOLUTIONS", async (entry) =>
            {
                return await GetCliBinder().ListVideoDeviceSupportResolutions(deviceName);
            });
        }

        public async Task<LibVersion> GetVersion()
        {
            return await _cache.GetOrCreateAsync($"FFMPEG_VERSION", async (entry) =>
            {
                return await GetCliBinder().GetVersion();
            });
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

        #region Log

        private LinkedList<FFMpegLog> _ffmegLogs = new LinkedList<FFMpegLog>();

        public IEnumerable<FFMpegLog> GetLogs()
        {
            if (_ffmegLogs.Any() != true)
            {
                return Enumerable.Empty<FFMpegLog>();
            }
            return _ffmegLogs;
        }

        public void AddLog(LogType logType, string message, Exception ex = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;
            while (_ffmegLogs.Count >= 300)
            {
                _ffmegLogs.RemoveLast();
            }
            if (message.Contains("frame=") && message.Contains("fps=") && _ffmegLogs.Count > 0)
            {
                var lastMessage = _ffmegLogs.First();
                if (lastMessage.Message.Contains("frame=") && lastMessage.Message.Contains("fps="))
                {
                    _ffmegLogs.RemoveFirst();
                }
            }
            _ffmegLogs.AddFirst(new FFMpegLog(logType, message, ex));
        }

        public void ClearLog()
        {
            _ffmegLogs.Clear();
        }

        #endregion
    }
}
