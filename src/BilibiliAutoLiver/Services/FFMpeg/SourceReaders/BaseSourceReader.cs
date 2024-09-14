using System;
using System.IO;
using System.Linq;
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

        public virtual ISourceReader WithInputArg()
        {
            GetVideoInputArg();
            GetAudioInputArg();
            return this;
        }

        public virtual FFMpegArgumentProcessor WithOutputArg()
        {
            if (FFMpegArguments == null)
                throw new Exception("请先指定输入参数");

            var rt = FFMpegArguments.OutputToUrl(RtmpAddr, opt =>
            {
                //音频
                GetAudioOutputArg(opt);

                //视频编码
                GetVideoOutputArg(opt);

                //PixelFormat
                opt.ForcePixelFormat("yuv420p");
                //输出格式
                opt.ForceFormat("flv");

                //推流分辨率
                if (this.Settings.PushSetting.OutputWidth > 0 && this.Settings.PushSetting.OutputHeight > 0)
                {
                    opt.Resize(this.Settings.PushSetting.OutputWidth, this.Settings.PushSetting.OutputHeight);
                }
            });
            return rt;
        }

        /// <summary>
        /// 获取视频输入参数
        /// </summary>
        protected abstract void GetVideoInputArg();

        /// <summary>
        /// 获取视频输出参数
        /// </summary>
        /// <param name="opt"></param>
        protected virtual void GetVideoOutputArg(FFMpegArgumentOptions opt)
        {
            var codec = GetVideoCodec();
            switch (this.Settings.PushSetting.Quality)
            {
                case OutputQualityEnum.High:
                    {
                        //opt.WithCustomArgument("-bufsize 10M");
                        opt.WithVideoCodec(codec);
                        opt.WithVideoBitrate(8000);
                        //用于设置 x264 编码器的编码速度和质量之间的权衡。
                        if (codec.Name.Equals("libx264") || codec.Name.Equals("libx265"))
                        {
                            opt.WithSpeedPreset(Speed.Medium);
                        }
                        //帧同步，可变帧率
                        opt.WithCustomArgument("-vsync vfr");
                        //指定 x264 编码器的调整参数，以优化特定类型的输入视频。
                        opt.WithCustomArgument("-tune zerolatency");
                        opt.WithCustomArgument("-g 30");
                        opt.WithConstantRateFactor(25);
                    }
                    break;
                default:
                case OutputQualityEnum.Medium:
                    {
                        //opt.WithCustomArgument("-bufsize 10M");
                        opt.WithVideoCodec(codec);
                        opt.WithVideoBitrate(4000);
                        //用于设置 x264 编码器的编码速度和质量之间的权衡。
                        if (codec.Name.Equals("libx264") || codec.Name.Equals("libx265"))
                        {
                            opt.WithSpeedPreset(Speed.Faster);
                        }
                        //帧同步，可变帧率
                        opt.WithCustomArgument("-vsync vfr");
                        //指定 x264 编码器的调整参数，以优化特定类型的输入视频。
                        opt.WithCustomArgument("-tune zerolatency");
                        opt.WithCustomArgument("-g 30");
                        opt.WithConstantRateFactor(20);
                    }
                    break;
                case OutputQualityEnum.Low:
                    {
                        //opt.WithCustomArgument("-bufsize 10M");
                        opt.WithVideoCodec(codec);
                        opt.WithVideoBitrate(2000);
                        //用于设置 x264 编码器的编码速度和质量之间的权衡。
                        if (codec.Name.Equals("libx264") || codec.Name.Equals("libx265"))
                        {
                            opt.WithSpeedPreset(Speed.SuperFast);
                        }
                        //帧同步，可变帧率
                        opt.WithCustomArgument("-vsync vfr");
                        //指定 x264 编码器的调整参数，以优化特定类型的输入视频。
                        opt.WithCustomArgument("-tune zerolatency");
                        opt.WithCustomArgument("-g 30");
                        opt.WithConstantRateFactor(15);
                    }
                    break;
                case OutputQualityEnum.Original:
                    {
                        opt.CopyChannel(Channel.Video);
                    }
                    break;
            }
        }

        /// <summary>
        /// 获取音频输入参数
        /// </summary>
        protected abstract void GetAudioInputArg();

        /// <summary>
        /// 获取音频输出参数
        /// </summary>
        /// <param name="opt"></param>
        protected virtual void GetAudioOutputArg(FFMpegArgumentOptions opt)
        {
            switch (this.Settings.PushSetting.Quality)
            {
                case OutputQualityEnum.High:
                    {
                        opt.WithAudioCodec(AudioCodec.Aac);
                        opt.WithAudioBitrate(AudioQuality.Ultra);
                        opt.WithAudioSamplingRate(48000);
                    }
                    break;
                default:
                case OutputQualityEnum.Medium:
                    {
                        opt.WithAudioCodec(AudioCodec.Aac);
                        opt.WithAudioBitrate(AudioQuality.Good);
                        opt.WithAudioSamplingRate(48000);
                    }
                    break;
                case OutputQualityEnum.Low:
                    {
                        opt.WithAudioCodec(AudioCodec.Aac);
                        opt.WithAudioBitrate(AudioQuality.Normal);
                        opt.WithAudioSamplingRate(44100);
                    }
                    break;
                case OutputQualityEnum.Original:
                    {
                        //opt.CopyChannel(Channel.Audio);
                    }
                    break;
            }
        }

        /// <summary>
        /// 是否包含音频流
        /// </summary>
        /// <returns></returns>
        protected abstract bool HasAudioStream();

        private Codec GetVideoCodec()
        {
            if (string.IsNullOrEmpty(this.Settings.PushSetting.CustumVideoCodec))
            {
                return VideoCodec.LibX264;
            }
            if (this.Settings.PushSetting.VideoCodecs?.Any() != true)
            {
                return VideoCodec.LibX264;
            }
            Codec target = this.Settings.PushSetting.VideoCodecs.FirstOrDefault(p => p.Name == this.Settings.PushSetting.CustumVideoCodec);
            if (target == null)
            {
                throw new Exception($"没有找到受支持的编码器名称：{this.Settings.PushSetting.CustumVideoCodec}");
            }
            return target;
        }

        public virtual void Dispose()
        {

        }
    }
}
