using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using BilibiliAutoLiver.Models.Settings;
using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Services.FFMpeg.SourceReaders
{
    public class CameraSourceReader : BaseSourceReader
    {
        public CameraSourceReader(LiveSettings settings, string rtmpAddr, ILogger logger) : base(settings, rtmpAddr, logger)
        {

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
                WithCommonOutputArg(opt);
            });
            return rt;
        }

        private void GetVideoInputArg()
        {
            InputVideoSource videoSource = Settings.V2.Input.VideoSource;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                FFMpegArguments = FFMpegArguments.FromFileInput($"video={Settings.V2.Input.VideoSource.Path}", false, opt =>
                {
                    opt.ForceFormat("dshow");
                    opt.WithFramerate(Settings.V2.Input.VideoSource.Framerate);
                    opt.WithCustomArgument($"-video_size {Settings.V2.Input.VideoSource.Width}x{Settings.V2.Input.VideoSource.Height}");
                    WithMuteArgument(opt);
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                FFMpegArguments = FFMpegArguments.FromFileInput($"video={Settings.V2.Input.VideoSource.Path}", false, opt =>
                {
                    opt.ForceFormat("v4l2");
                    opt.WithFramerate(Settings.V2.Input.VideoSource.Framerate);
                    opt.WithCustomArgument($"-video_size {Settings.V2.Input.VideoSource.Width}x{Settings.V2.Input.VideoSource.Height}");
                    WithMuteArgument(opt);
                });
            }
            else
            {
                throw new NotSupportedException("不支持的系统类型");
            }
        }
    }
}
