using System;
using System.IO;
using BilibiliAutoLiver.Models.Settings;
using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Services.FFMpeg.SourceReaders
{
    public abstract class BaseSourceReader : ISourceReader
    {
        protected readonly ILogger _logger;   

        protected LiveSettings Settings { get; set; }
        protected FFMpegArguments FFMpegArguments { get; set; }

        protected FFMpegArgumentProcessor Processor { get; set; }

        protected string RtmpAddr { get; set; }

        protected string videoMuteMapOpt = null;
        protected string audioMuteMapOpt = null;

        public BaseSourceReader(LiveSettings settings, string rtmpAddr, ILogger logger)
        {
            this.Settings = settings;
            this.RtmpAddr = !rtmpAddr.StartsWith('\"') ? $"\"{rtmpAddr}\"" : rtmpAddr;
            _logger = logger;
        }

        public abstract ISourceReader WithInputArg();

        public abstract FFMpegArgumentProcessor WithOutputArg();

        protected void GetAudioInputArg()
        {
            if (!HasAudio())
            {
                return;
            }
            var fullPath = Path.GetFullPath(this.Settings.V2.Input.AudioSource.Path);
            this.FFMpegArguments.AddFileInput(fullPath, true, opt =>
            {
                opt.WithCustomArgument("-stream_loop -1");
                audioMuteMapOpt = "-map 1:a:0";
            });
        }

        protected bool HasAudio()
        {
            if (this.Settings.V2.Input.AudioSource == null)
            {
                return false;
            }
            if (this.Settings.V2.Input.AudioSource.Type == Models.Enums.InputSourceType.None)
            {
                return false;
            }
            if (string.IsNullOrEmpty(this.Settings.V2.Input.AudioSource.Path))
            {
                throw new ArgumentNullException("音频输入源Path不能为空");
            }
            if (!File.Exists(this.Settings.V2.Input.AudioSource.Path))
            {
                throw new FileNotFoundException($"音频输入源{this.Settings.V2.Input.AudioSource.Path}文件不存在", this.Settings.V2.Input.AudioSource.Path);
            }
            return true;
        }

        protected void WithMuteArgument(FFMpegArgumentOptions opt)
        {
            if (this.Settings.V2.Input.VideoSource.IsMute)
            {
                if (HasAudio())
                {
                    videoMuteMapOpt = "-map 0:v:0";
                }
                else
                {
                    opt.DisableChannel(Channel.Audio);
                }
            }
        }

        public virtual void Dispose()
        {
            
        }
    }
}
