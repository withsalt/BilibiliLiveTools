using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models;
using CliWrap;
using CliWrap.Buffered;

namespace BilibiliAutoLiver.Services.FFMpeg.Services.CliBinder
{
    public abstract class BaseFFMpegCliBinder : IFFMpegCliBinder
    {
        public string FFMpegPath { get; }

        public string WorkingDirectory { get; }

        public BaseFFMpegCliBinder(string ffmpegPath, string workingDirectory)
        {
            this.FFMpegPath = ffmpegPath;
            this.WorkingDirectory = workingDirectory;
        }

        public abstract Task<List<string>> GetVideoDevices();

        public abstract Task<List<string>> GetAudioDevices();

        public async Task<LibVersion> GetVersion()
        {
            LibVersion libVersion = new LibVersion();
            var result = await Cli.Wrap(this.FFMpegPath)
                .WithArguments("-version")
                .WithWorkingDirectory(this.WorkingDirectory)
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync(Encoding.UTF8);
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
    }
}
