using System;
using System.Linq;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Models.Settings;
using BilibiliAutoLiver.Services.FFMpeg.Ext;
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
                GetAudioOutputArg(opt);
                GetVideoOutputArg(opt);
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
            Codec codec = GetVideoCodec();
            QualitySettings qualitySettings = null;
            switch (this.Settings.PushSetting.Quality)
            {
                case OutputQualityEnum.High:
                    {
                        qualitySettings = this.Settings.AppSettings.FFmpegPresetParams.OutputQuality.High;
                    }
                    break;
                default:
                case OutputQualityEnum.Medium:
                    {
                        qualitySettings = this.Settings.AppSettings.FFmpegPresetParams.OutputQuality.Medium;
                    }
                    break;
                case OutputQualityEnum.Low:
                    {
                        qualitySettings = this.Settings.AppSettings.FFmpegPresetParams.OutputQuality.Low;
                    }
                    break;
                case OutputQualityEnum.Original:
                    {
                        opt.CopyChannel(Channel.Video);
                    }
                    break;
            }
            if (qualitySettings != null)
            {
                if (!string.IsNullOrWhiteSpace(qualitySettings.BufferSize))
                {
                    opt.WithCustomArgument($"-bufsize {qualitySettings.BufferSize}");
                }
                opt.WithVideoCodec(codec);
                opt.WithVideoBitrate(qualitySettings.Bitrate);
                opt.WithCustomArgument($"-maxrate {qualitySettings.Bitrate}k");
                //用于设置 x264 编码器的编码速度和质量之间的权衡。
                opt.Withx264Orx265SpeedPreset(codec, qualitySettings.SpeedPreset);
                if (!string.IsNullOrWhiteSpace(qualitySettings.FpsMode))
                {
                    //帧同步，可变帧率
                    opt.WithCustomArgument($"-fps_mode {qualitySettings.FpsMode}");
                }
                if (qualitySettings.ZeroLatency)
                {
                    //指定 x264 编码器的调整参数，以优化特定类型的输入视频。
                    opt.WithCustomArgument("-tune zerolatency");
                }
                if (qualitySettings.GOP > 0)
                {
                    //用于设置视频编码时的关键帧间隔（GOP, Group of Pictures）。这个参数定义了两个关键帧之间的帧数。
                    opt.WithCustomArgument($"-g {qualitySettings.GOP}");
                }
                opt.WithConstantRateFactor(qualitySettings.ConstantRateFactor);
                //额外的自定义参数
                if (!string.IsNullOrWhiteSpace(qualitySettings.CustomArgument))
                {
                    opt.WithCustomArgument(qualitySettings.CustomArgument);
                }
            }

            //PixelFormat
            opt.ForcePixelFormat(!string.IsNullOrWhiteSpace(this.Settings.AppSettings.FFmpegPresetParams.PixelFormat) ? this.Settings.AppSettings.FFmpegPresetParams.PixelFormat : "yuv420p");

            //延迟参数
            if (this.Settings.AppSettings.FFmpegPresetParams.LowDelayFlags)
            {
                opt.WithLowDelayArgument();
            }

            //推流分辨率
            if (this.Settings.PushSetting.OutputWidth > 0 && this.Settings.PushSetting.OutputHeight > 0)
            {
                opt.Resize(this.Settings.PushSetting.OutputWidth, this.Settings.PushSetting.OutputHeight);
            }

            //输出格式
            opt.ForceFormat(!string.IsNullOrWhiteSpace(this.Settings.AppSettings.FFmpegPresetParams.Format) ? this.Settings.AppSettings.FFmpegPresetParams.Format : "flv");

            //自定义参数
            if (!string.IsNullOrWhiteSpace(this.Settings.PushSetting.CustumOutputParams))
            {
                opt.WithCustomArgument(this.Settings.PushSetting.CustumOutputParams);
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
                var libx264 = this.Settings.PushSetting.VideoCodecs.FirstOrDefault(v => v.Name == "libx264");
                if (libx264 != null)
                {
                    return libx264;
                }
                var h264 = this.Settings.PushSetting.VideoCodecs.FirstOrDefault(v => v.Name == "h264");
                if (h264 != null)
                {
                    return h264;
                }
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
