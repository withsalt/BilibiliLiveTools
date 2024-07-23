using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;

namespace BilibiliAutoLiver.Services.FFMpeg.Services.CliBinder
{
    public class FFMpegWindowsCliBinder : BaseFFMpegCliBinder
    {
        private string _excuteResult = null;

        public FFMpegWindowsCliBinder(string ffmpegPath, string workingDirectory) : base(ffmpegPath, workingDirectory)
        {

        }

        public override async Task<List<string>> GetVideoDevices()
        {
            string output = await GetExcuteResult();
            List<string> devices = ExtractDevices(output, "video");
            return devices;
        }

        public override async Task<List<string>> GetAudioDevices()
        {
            string output = await GetExcuteResult();
            List<string> devices = ExtractDevices(output, "audio");
            return devices;
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
                .ExecuteBufferedAsync(Encoding.UTF8);
            if (!string.IsNullOrWhiteSpace(result.StandardOutput))
            {
                _excuteResult = result.StandardOutput;
                return _excuteResult;
            }
            if (!string.IsNullOrWhiteSpace(result.StandardError))
            {
                _excuteResult = result.StandardError;
                return _excuteResult;
            }
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
            List<string> devices = new List<string>();

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
                        devices.Add(deviceName);
                    }
                }
            }
            return devices;
        }
    }
}
