using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using BilibiliAutoLiver.Models.Settings;
using BilibiliAutoLiver.Plugin.Base;
using BilibiliAutoLiver.Services.FFMpeg.DeviceProviders;
using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Services.FFMpeg.SourceReaders
{
    public class DeviceSourceReader : BaseSourceReader
    {
        private ICameraDeviceProvider DeviceProvider { get; set; }
        private IPipeContainer PipeContainer { get; set; }

        public DeviceSourceReader(LiveSettings settings, string rtmpAddr, ILogger logger, IPipeContainer pipeContainer) : base(settings, rtmpAddr, logger)
        {
            this.PipeContainer = pipeContainer;
            this.DeviceProvider = new CameraDeviceProvider(settings.V2.Input.VideoSource, p =>
            {

            });
            this.DeviceProvider.Start();
        }

        public override ISourceReader WithInputArg()
        {
            GetVideoInputArg();
            GetAudioInputArg();
            return this;
        }

        public override FFMpegArgumentProcessor WithOutputArg()
        {
            if (FFMpegArguments == null) throw new Exception("请先指定输入参数");
            var rt = FFMpegArguments.OutputToUrl(RtmpAddr, opt =>
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
                }
                //视频编码
                opt.WithVideoCodec(VideoCodec.LibX264);
                opt.ForceFormat("flv");
                opt.ForcePixelFormat("yuv420p");
                opt.WithConstantRateFactor(20);
                opt.UsingShortest();
            });
            return rt;
        }

        private void GetVideoInputArg()
        {

        }

        private void WithMuteArgument(InputVideoSource videoSource, FFMpegArgumentOptions opt)
        {
            if (videoSource.IsMute)
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

        public override void Dispose()
        {
            if (this.DeviceProvider != null)
                this.DeviceProvider.Dispose();
        }
    }
}
