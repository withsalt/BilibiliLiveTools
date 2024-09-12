using System.IO;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Models.Enums;
using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Services.FFMpeg.SourceReaders
{
    public abstract class BaseSourceReader : ISourceReader
    {
        protected readonly ILogger _logger;

        protected SettingDto Settings { get; set; }
        protected FFMpegArguments FFMpegArguments { get; set; }

        protected FFMpegArgumentProcessor Processor { get; set; }

        protected string RtmpAddr { get; set; }

        public BaseSourceReader(SettingDto setting, string rtmpAddr, ILogger logger)
        {
            this.Settings = setting;
            this.RtmpAddr = !rtmpAddr.StartsWith('\"') ? $"\"{rtmpAddr}\"" : rtmpAddr;
            _logger = logger;
        }

        public abstract ISourceReader WithInputArg();

        public abstract FFMpegArgumentProcessor WithOutputArg();

        protected void WithCommonOutputArg(FFMpegArgumentOptions opt)
        {
            //音频
            WithAudioArgument(opt);

            //音频编码
            if (HasAudio())
            {
                opt.WithAudioCodec(AudioCodec.Aac);
                opt.WithAudioSamplingRate(44100);
                opt.WithAudioBitrate(AudioQuality.Normal);
            }
            opt.ForcePixelFormat("yuv420p");
            //视频编码
            WithQualityOutputArg(opt);
            //输出格式
            opt.ForceFormat("flv");
            //opt.ForcePixelFormat("yuv420p");

            //推流分辨率
            if (this.Settings.PushSettingDto.OutputWidth > 0 && this.Settings.PushSettingDto.OutputHeight > 0)
            {
                opt.Resize(this.Settings.PushSettingDto.OutputWidth, this.Settings.PushSettingDto.OutputHeight);
            }
        }

        protected virtual void WithQualityOutputArg(FFMpegArgumentOptions opt)
        {
            switch (this.Settings.PushSettingDto.Quality)
            {
                case OutputQualityEnum.High:
                    {
                        //opt.WithCustomArgument("-bufsize 10M");
                        opt.WithVideoCodec(VideoCodec.LibX264);
                        opt.WithVideoBitrate(12288);
                        //用于设置 x264 编码器的编码速度和质量之间的权衡。
                        opt.WithSpeedPreset(Speed.Medium);
                        //指定 x264 编码器的调整参数，以优化特定类型的输入视频。
                        opt.WithCustomArgument("-tune zerolatency");
                        opt.WithCustomArgument("-g 30");
                        opt.WithConstantRateFactor(20);
                    }
                    break;
                default:
                case OutputQualityEnum.Medium:
                    {
                        //opt.WithCustomArgument("-bufsize 10M");
                        opt.WithVideoCodec(VideoCodec.LibX264);
                        opt.WithVideoBitrate(8192);
                        //用于设置 x264 编码器的编码速度和质量之间的权衡。
                        opt.WithSpeedPreset(Speed.Faster);
                        //指定 x264 编码器的调整参数，以优化特定类型的输入视频。
                        opt.WithCustomArgument("-tune zerolatency");
                        opt.WithCustomArgument("-g 30");
                        opt.WithConstantRateFactor(20);
                    }
                    break;
                case OutputQualityEnum.Low:
                    {
                        //opt.WithCustomArgument("-bufsize 10M");
                        opt.WithVideoCodec(VideoCodec.LibX264);
                        opt.WithVideoBitrate(6144);
                        //用于设置 x264 编码器的编码速度和质量之间的权衡。
                        opt.WithSpeedPreset(Speed.SuperFast);
                        //指定 x264 编码器的调整参数，以优化特定类型的输入视频。
                        opt.WithCustomArgument("-tune zerolatency");
                        opt.WithCustomArgument("-g 30");
                        opt.WithConstantRateFactor(20);
                    }
                    break;
                case OutputQualityEnum.Original:
                    {
                        opt.CopyChannel(Channel.Video);
                    }
                    break;
            }
        }

        protected void GetAudioInputArg()
        {
            if (!HasAudio())
            {
                return;
            }
            this.FFMpegArguments.AddFileInput(this.Settings.PushSettingDto.AudioInfo.FullPath, true, opt =>
            {
                opt.WithCustomArgument("-stream_loop -1");
            });
        }

        protected bool HasAudio()
        {
            if (this.Settings.PushSettingDto.AudioInfo == null || string.IsNullOrWhiteSpace(this.Settings.PushSettingDto.AudioInfo.FullPath))
            {
                return false;
            }
            if (!File.Exists(this.Settings.PushSettingDto.AudioInfo.FullPath))
            {
                throw new FileNotFoundException($"音频输入源{this.Settings.PushSettingDto.AudioInfo.FullPath}文件不存在", this.Settings.PushSettingDto.AudioInfo.FullPath);
            }
            return true;
        }

        protected void WithMuteArgument(FFMpegArgumentOptions opt)
        {
            if (this.Settings.PushSettingDto.IsMute)
            {
                opt.DisableChannel(Channel.Audio);
            }
        }

        protected void WithAudioArgument(FFMpegArgumentOptions opt)
        {
            if (!HasAudio())
            {
                return;
            }
            if (this.Settings.PushSettingDto.IsMute)
            {
                //视频静音，但是包含音频
                opt.WithCustomArgument($"-map 0:v:{this.Settings.PushSettingDto.VideoInfo.MediaInfo.PrimaryIndex}");
                opt.WithCustomArgument($"-map 1:a:{this.Settings.PushSettingDto.AudioInfo.MediaInfo.PrimaryIndex}");
            }
            else
            {

            }
            opt.UsingShortest();
        }

        public virtual void Dispose()
        {

        }
    }
}
