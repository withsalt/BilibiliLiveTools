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
        protected FFMpegArguments FFMpegArguments { get; set; }

        protected FFMpegArgumentProcessor Processor { get; set; }

        protected string RtmpAddr { get; set; }

        protected string videoMuteMapOpt = null;
        protected string audioMuteMapOpt = null;

        public BaseSourceReader(LiveSettings settings, string rtmpAddr)
        {
            this.Settings = settings;
            this.RtmpAddr = !rtmpAddr.StartsWith('\"') ? $"\"{rtmpAddr}\"" : rtmpAddr;
        }

        public abstract ISourceReader WithInputArg();

        public abstract FFMpegArgumentProcessor WithOutputArg();

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
    }
}
