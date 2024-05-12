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

        protected void WithCommonOutputArg(FFMpegArgumentOptions opt)
        {
            //禁用视频中的音频
            if (!string.IsNullOrEmpty(videoMuteMapOpt))
            {
                opt.WithCustomArgument(videoMuteMapOpt);
            }
            //禁用音频中的视频
            if (!string.IsNullOrEmpty(audioMuteMapOpt))
            {
                opt.WithCustomArgument(audioMuteMapOpt);
            }
            //音频编码
            if (HasAudio())
            {
                opt.WithAudioCodec(AudioCodec.Aac);
                opt.WithAudioSamplingRate(44100);
                opt.WithAudioBitrate(AudioQuality.Normal);
            }
            //视频编码
            //opt.WithCustomArgument("-bufsize 10M");
            opt.WithVideoCodec(VideoCodec.LibX264);
            opt.ForceFormat("flv");
            opt.ForcePixelFormat("yuv420p");
            opt.WithConstantRateFactor(23);
            opt.WithVideoBitrate(6000);
            //用于设置 x264 编码器的编码速度和质量之间的权衡。
            opt.WithSpeedPreset(Speed.SuperFast);
            //指定 x264 编码器的调整参数，以优化特定类型的输入视频。
            opt.WithCustomArgument("-tune zerolatency");
            opt.WithCustomArgument("-g 30");

            //推流分辨率
            if (Settings.V2.Output.Height > 0 && Settings.V2.Output.Width > 0)
            {
                opt.Resize(Settings.V2.Output.Width, Settings.V2.Output.Height);
            }
        }

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
