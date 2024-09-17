using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public override async Task<List<string>> GetVideoDevices()
        {
            List<string> devices = new List<string>();
            return devices;
        }

        public override async Task<List<string>> GetAudioDevices()
        {
            List<string> devices = new List<string>();
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

        private async Task<string> GetExcuteResult()
        {
            if (!string.IsNullOrWhiteSpace(_excuteResult))
            {
                return _excuteResult;
            }
            var result = await Cli.Wrap(this.FFMpegPath)
                .WithArguments("-list_devices true -f dshow -i dummy")
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
