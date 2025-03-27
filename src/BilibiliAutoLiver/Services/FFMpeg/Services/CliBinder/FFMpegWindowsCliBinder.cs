using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Models.FFMpeg;
using BilibiliAutoLiver.Utils;
using CliWrap;
using CliWrap.Buffered;
using FlashCap;

namespace BilibiliAutoLiver.Services.FFMpeg.Services.CliBinder
{
    public class FFMpegWindowsCliBinder : BaseFFMpegCliBinder
    {
        public FFMpegWindowsCliBinder(string ffmpegPath, string workingDirectory) : base(ffmpegPath, workingDirectory)
        {

        }

        public override async Task<List<VideoDeviceInfo>> GetVideoDevices()
        {
            string output = await GetExcuteResult("-list_devices true -f dshow -i dummy");
            List<(string type, string name)> devices = ExtractDevices(output, "video");
            List<VideoDeviceInfo> deviceInfos = new List<VideoDeviceInfo>();
            if (devices?.Any() != true)
            {
                return deviceInfos;
            }
            foreach (var item in devices)
            {
                deviceInfos.Add(new VideoDeviceInfo()
                {
                    Name = item.name,
                    DeviceType = DeviceTypes.DirectShow,
                    Identity = item.name
                });
            }
            return deviceInfos;
        }

        public override async Task<List<AudioDeviceInfo>> GetAudioDevices()
        {
            string output = await GetExcuteResult("-list_devices true -f dshow -i dummy");
            List<(string type, string name)> devices = ExtractDevices(output, "audio");
            List<AudioDeviceInfo> deviceInfos = new List<AudioDeviceInfo>();
            if (devices?.Any() != true)
            {
                return deviceInfos;
            }
            foreach (var item in devices)
            {
                deviceInfos.Add(new AudioDeviceInfo()
                {
                    Name = item.name,
                    DeviceType = AudioDeviceType.DirectShow,
                    CardIndex = -1,
                    DeviceIndex = -1,
                });
            }
            return deviceInfos;
        }

        public override async Task<List<DeviceResolution>> ListVideoDeviceSupportResolutions(string deviceName)
        {
            if (string.IsNullOrWhiteSpace(deviceName))
            {
                throw new ArgumentNullException("deviceName", "设备名称不能为空");
            }
            string argStr = $"-f dshow -list_options true -i video=\"{deviceName}\"";
            string output = await GetExcuteResult(argStr);
            List<DeviceResolution> devices = ExtractResolutions(output, deviceName);
            return devices;
        }

        private async Task<string> GetExcuteResult(string args)
        {
            var result = await Cli.Wrap(this.FFMpegPath)
                .WithArguments(args)
                .WithWorkingDirectory(this.WorkingDirectory)
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync(Encoding.UTF8);
            var _excuteResult = !string.IsNullOrWhiteSpace(result.StandardOutput)
                ? result.StandardOutput
                : result.StandardError;
            return _excuteResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ffmpegOutput"></param>
        /// <param name="type">audio/video</param>
        /// <returns></returns>
        private List<(string type, string name)> ExtractDevices(string ffmpegOutput, string type)
        {
            if (string.IsNullOrEmpty(ffmpegOutput))
            {
                throw new ArgumentNullException(nameof(ffmpegOutput), "FFMpeg输出内容为空");
            }
            string[] lines = ffmpegOutput.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            List<(string, string)> devices = new List<(string, string)>();

            foreach (var line in lines)
            {
                if (line.Contains($"Could not enumerate {type}"))
                {
                    return devices;
                }
                if (line.Contains("dshow") && line.Contains($"({type})") && line.Contains("\""))
                {
                    int firstQuote = line.IndexOf("\"");
                    int lastQuote = line.LastIndexOf("\"");
                    if (firstQuote != lastQuote)
                    {
                        string deviceName = line.Substring(firstQuote + 1, lastQuote - firstQuote - 1).Trim(' ');
                        devices.Add(("dshow", deviceName));
                    }
                }
            }
            return devices;
        }

        private List<DeviceResolution> ExtractResolutions(string ffmpegOutput, string deviceName)
        {
            if (string.IsNullOrEmpty(ffmpegOutput))
            {
                throw new ArgumentNullException(nameof(ffmpegOutput), "FFMpeg输出内容为空");
            }
            if (ffmpegOutput.Contains("Could not find video device", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"没有找到设备【{deviceName}】");
            }
            if (ffmpegOutput.Contains("Error opening input: I/O error", StringComparison.OrdinalIgnoreCase)
                && ffmpegOutput.Contains("Error opening input file video", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"没有找到设备【{deviceName}】，或设备无法访问");
            }
            string[] lines = ffmpegOutput.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            List<DeviceResolution> resolutions = new List<DeviceResolution>();

            foreach (var item in lines)
            {
                if (string.IsNullOrWhiteSpace(item)) continue;
                if (!item.StartsWith("[dshow", StringComparison.OrdinalIgnoreCase)) continue;
                if (item.Contains("(pc")) continue;

                (string type, string format) = GetFormatFromLine(item);
                if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(format)) continue;

                if (item.Contains("min s="))
                {
                    int firstIndex = item.IndexOf("min s=") + 6;
                    int endIndex = item.IndexOf(' ', firstIndex);
                    if (firstIndex < endIndex)
                    {
                        string resolution = item.Substring(firstIndex, endIndex - firstIndex);
                        if (!string.IsNullOrWhiteSpace(resolution) && ResolutionHelper.TryParse(resolution, out int width, out int height))
                        {
                            resolutions.Add(new DeviceResolution(type, format, width, height));
                            continue;
                        }
                    }
                }
                if (item.Contains("max s"))
                {
                    int firstIndex = item.IndexOf("max s") + 6;
                    int endIndex = item.IndexOf(' ', firstIndex);
                    if (firstIndex < endIndex)
                    {
                        string resolution = item.Substring(firstIndex, endIndex - firstIndex);
                        if (!string.IsNullOrWhiteSpace(resolution) && ResolutionHelper.TryParse(resolution, out int width, out int height))
                        {
                            resolutions.Add(new DeviceResolution(type, format, width, height));
                            continue;
                        }
                    }
                }
            }
            if (resolutions.Count == 0)
            {
                return new List<DeviceResolution>();
            }
            List<DeviceResolution> result = resolutions
                .OrderBy(p => p.Format)
                .ThenBy(p => p.Width)
                .ThenBy(p => p.Height)
                .ToList();
            return result;
        }

        private (string type, string format) GetFormatFromLine(string line)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(line))
                    return (null, null);
                int vcodecIndex = line.IndexOf("vcodec=");
                int pixelFormatIndex = line.IndexOf("pixel_format=");
                if (vcodecIndex < 0 && pixelFormatIndex < 0)
                {
                    return (null, null);
                }
                int firstIndex = vcodecIndex >= 0 ? vcodecIndex : pixelFormatIndex;
                int nextSpaceCharIndex = line.IndexOf(' ', firstIndex);
                if (firstIndex >= 0 && nextSpaceCharIndex > 0 && nextSpaceCharIndex > firstIndex && nextSpaceCharIndex - firstIndex > 7)
                {
                    string formatLine = line.Substring(firstIndex, nextSpaceCharIndex - firstIndex)?.Trim(' ');
                    if (string.IsNullOrWhiteSpace(formatLine))
                    {
                        return (null, null);
                    }
                    string[] args = formatLine.Split('=');
                    if (args.Length != 2)
                    {
                        return (null, null);
                    }
                    return (args[0], args[1]);
                }

                return (null, null);
            }
            catch
            {
                return (null, null);
            }
        }
    }
}
