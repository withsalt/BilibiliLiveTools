using System;
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
                WithAudioArgument(opt);
                
                //视频编码
                WithQualityOutputArg(opt);

                //PixelFormat
                opt.ForcePixelFormat("yuv420p");
                //输出格式
                opt.ForceFormat("flv");

                //推流分辨率
                if (this.Settings.PushSettingDto.OutputWidth > 0 && this.Settings.PushSettingDto.OutputHeight > 0)
                {
                    opt.Resize(this.Settings.PushSettingDto.OutputWidth, this.Settings.PushSettingDto.OutputHeight);
                }
            });
            return rt;
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

        /// <summary>
        /// 获取视频输入参数
        /// </summary>
        protected abstract void GetVideoInputArg();

        /// <summary>
        /// 获取音频输入参数
        /// </summary>
        protected abstract void GetAudioInputArg();

        /// <summary>
        /// 是否包含音频流
        /// </summary>
        /// <returns></returns>
        protected abstract bool HasAudioStream();

        protected virtual void WithMuteArgument(FFMpegArgumentOptions opt)
        {

        }

        protected virtual void WithAudioArgument(FFMpegArgumentOptions opt)
        {
            opt.WithAudioCodec(AudioCodec.Aac);
            opt.WithAudioSamplingRate(44100);
            opt.WithAudioBitrate(AudioQuality.Normal);
        }

        public virtual void Dispose()
        {

        }
    }
}
