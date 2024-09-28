using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Models.FFMpeg;
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
            return new List<DeviceResolution>();
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
    }
}
