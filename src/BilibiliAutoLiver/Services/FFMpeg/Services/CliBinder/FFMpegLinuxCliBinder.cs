using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Models.FFMpeg;
using BilibiliAutoLiver.Utils;
using CliWrap;
using CliWrap.Buffered;

namespace BilibiliAutoLiver.Services.FFMpeg.Services.CliBinder
{
    public class FFMpegLinuxCliBinder : BaseFFMpegCliBinder
    {
        private string _excuteResult = null;

        public FFMpegLinuxCliBinder(string ffmpegPath, string workingDirectory) : base(ffmpegPath, workingDirectory)
        {

        }

        public override Task<List<VideoDeviceInfo>> GetVideoDevices()
        {
            throw new NotSupportedException("暂不支持在Linux操作系统上面，通过FFMpeg获取视频输入设备。");
        }

        public override async Task<List<AudioDeviceInfo>> GetAudioDevices()
        {
            List<AudioDeviceInfo> devices = new List<AudioDeviceInfo>();

            string output = await GetExcuteResult("arecord", "-l");
            if (string.IsNullOrWhiteSpace(output))
            {
                return devices;
            }

            string[] outputArgs = output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            List<string> cardLines = new List<string>();
            foreach (var outputItem in outputArgs)
            {
                string outputItemNew = outputItem.Trim('\r', '\n', ' ');
                if (!string.IsNullOrWhiteSpace(outputItemNew) && outputItemNew.StartsWith("card"))
                {
                    cardLines.Add(outputItemNew);
                }
            }
            if (cardLines.Count == 0)
            {
                return devices;
            }

            string pattern = @"card (\d+): ([^]]+) \[([^]]+)\], device (\d+): ([^]]+) \[([^]]+)\]";
            foreach (var cardLine in cardLines)
            {
                var match = Regex.Match(cardLine, pattern);
                if (match.Success
                    && match.Groups.Count > 6
                    && int.TryParse(match.Groups[1].Value, out int cardIndex)
                    && int.TryParse(match.Groups[4].Value, out int deviceIndex)
                    && cardIndex >= 0
                    && deviceIndex >= 0)
                {
                    AudioDeviceInfo deviceInfo = new AudioDeviceInfo
                    {
                        CardIndex = cardIndex,
                        Name = match.Groups[3].Value,
                        DeviceIndex = deviceIndex,
                        DeviceName = match.Groups[6].Value,
                        DeviceType = AudioDeviceType.Alsa,
                    };
                    devices.Add(deviceInfo);
                }
            }
            return devices;
        }

        public override async Task<List<DeviceResolution>> ListVideoDeviceSupportResolutions(string deviceName)
        {
            if (string.IsNullOrWhiteSpace(deviceName))
            {
                throw new ArgumentNullException("deviceName", "设备名称不能为空");
            }
            string argStr = $"f v4l2 -list_formats all -i \"/{deviceName}\"";
            string output = await GetExcuteResult(this.FFMpegPath, argStr);
            List<DeviceResolution> devices = ExtractResolutions(output, deviceName);
            return devices;
        }

        private async Task<string> GetExcuteResult(string name, string args)
        {
            if (!string.IsNullOrWhiteSpace(_excuteResult))
            {
                return _excuteResult;
            }
            var result = await Cli.Wrap(name)
                .WithArguments(args)
                .WithWorkingDirectory(this.WorkingDirectory)
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();
            _excuteResult = result.StandardOutput;
            return _excuteResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ffmpegOutput"></param>
        /// <param name="type">audio/video</param>
        /// <returns></returns>
        private List<string> ExtractDevices(string ffmpegOutput, string type)
        {
            string[] lines = ffmpegOutput.Split('\n');
            bool audioSection = false;

            List<string> devices = new List<string>();

            foreach (var line in lines)
            {
                if (line.Contains($"({type})") && line.Contains("\""))
                {
                    int firstQuote = line.IndexOf("\"");
                    int lastQuote = line.LastIndexOf("\"");
                    if (firstQuote != lastQuote)
                    {
                        string deviceName = line.Substring(firstQuote + 1, lastQuote - firstQuote - 1);
                        devices.Add(deviceName);
                    }
                }
                else if (audioSection)
                {
                    // 提取设备名称，假设它总是跟在 '(audio)' 后面
                    int start = line.IndexOf(" \"") + 2;
                    int end = line.IndexOf("\" (audio)");
                    if (start >= 0 && end > start)
                    {
                        string deviceName = line.Substring(start, end - start);
                        devices.Add(deviceName);
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
            if (ffmpegOutput.Contains("No such file or directory", StringComparison.OrdinalIgnoreCase))
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
                if (!item.StartsWith("[video4linux2", StringComparison.OrdinalIgnoreCase)) continue;
                if (!item.Contains("Raw") && !item.Contains("Compressed")) continue;

                int rowIndex = item.IndexOf("Raw");
                if (rowIndex > 0)
                {
                    (int startIndex, string format) = GetFormatFromLine(item, rowIndex);
                    if (startIndex > 0)
                    {
                        List<(int width, int height)> resolutionItems = GetResolutionsFromLine(item, startIndex);
                        if (resolutionItems?.Any() == true)
                        {
                            foreach (var resolutionItem in resolutionItems)
                            {
                                resolutions.Add(new DeviceResolution("row", format, resolutionItem.width, resolutionItem.height));
                            }
                        }
                    }
                }
                int compressedIndex = item.IndexOf("Compressed");
                if (compressedIndex > 0)
                {
                    (int startIndex, string format) = GetFormatFromLine(item, compressedIndex);
                    if (startIndex > 0)
                    {
                        var resolutionItems = GetResolutionsFromLine(item, startIndex);
                        if (resolutionItems?.Any() == true)
                        {
                            foreach (var resolutionItem in resolutionItems)
                            {
                                resolutions.Add(new DeviceResolution("row", format, resolutionItem.width, resolutionItem.height));
                            }
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

        private (int index, string format) GetFormatFromLine(string item, int startIndex)
        {
            if (startIndex <= 0)
            {
                return (-1, null);
            }

            int next = item.IndexOf(':', startIndex + 3);
            if (next > startIndex)
            {
                int nextNext = item.IndexOf(":", next + 1);
                if (nextNext > next && nextNext - next > 3)
                {
                    string format = item.Substring(next + 1, nextNext - next - 1);
                    if (!string.IsNullOrWhiteSpace(format))
                    {
                        format = format.Trim('\r', '\n', ' ');
                        return (nextNext, format);
                    }
                }
            }

            return (-1, null);
        }

        private List<(int width, int height)> GetResolutionsFromLine(string item, int startIndex)
        {
            List<(int width, int height)> result = new List<(int width, int height)>();
            if (startIndex <= 0)
            {
                return result;
            }
            // 匹配宽x高的分辨率
            string pattern = @"\b\d+x\d+\b";
            MatchCollection matches = Regex.Matches(item.Substring(startIndex), pattern);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    string[] arg = match.Value.Split('x');
                    if (arg.Length != 2)
                    {
                        continue;
                    }
                    if (!string.IsNullOrEmpty(match.Value) && ResolutionHelper.TryParse(match.Value, out int width, out int height))
                    {
                        result.Add((width, height));
                    }
                }
            }
            return result;
        }
    }
}
