using System;
using System.IO;
using BilibiliAutoLiver.Models;
using FFMpegCore;
using FFMpegCore.Enums;

namespace BilibiliAutoLiver.Services.SourceReaders
{
    public abstract class BaseSourceReader : ISourceReader
    {
        protected LiveSettings Settings { get; set; }

        protected string videoMuteMapOpt = null;
        protected string audioMuteMapOpt = null;

        public BaseSourceReader(LiveSettings settings)
        {
            this.Settings = settings;
        }

        public abstract FFMpegArguments BuildInputArg();

        public virtual void WithDisableAudioChannel(FFMpegArgumentOptions opt)
        {
            if (!string.IsNullOrEmpty(videoMuteMapOpt))
            {
                opt.WithCustomArgument(videoMuteMapOpt);
            }
        }

        public virtual void WithDisableVideoChannel(FFMpegArgumentOptions opt)
        {
            if (!string.IsNullOrEmpty(audioMuteMapOpt))
            {
                opt.WithCustomArgument(audioMuteMapOpt);
            }
        }

        public virtual void WithAudioCodec(FFMpegArgumentOptions opt)
        {
            if (HasAudio())
            {
                opt.WithAudioCodec(AudioCodec.Aac);
            }
        }

        public virtual void WithVideoCodec(FFMpegArgumentOptions opt)
        {
            opt.WithVideoCodec(VideoCodec.LibX264);
        }

        protected bool HasAudio()
        {
            if (this.Settings.V2.Input.AudioSource == null)
            {
                return false;
            }
            if(this.Settings.V2.Input.AudioSource.Type == Models.Enums.InputSourceType.None)
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
    }
}
