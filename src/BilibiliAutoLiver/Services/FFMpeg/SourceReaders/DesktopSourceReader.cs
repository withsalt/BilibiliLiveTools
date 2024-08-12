using System;
using System.Drawing;
using System.Runtime.InteropServices;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Utils;
using FFMpegCore;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Services.FFMpeg.SourceReaders
{
    public class DesktopSourceReader : BaseSourceReader
    {
        public DesktopSourceReader(SettingDto setting, string rtmpAddr, ILogger logger) : base(setting, rtmpAddr, logger)
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
            if (!ScreenParamsHelper.TryParse(this.Settings.PushSettingDto.InputScreen, out string message, out Rectangle? rectangle))
            {
                throw new Exception(message);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                FFMpegArguments = FFMpegArguments.FromFileInput("desktop", false, opt =>
                {
                    opt.ForceFormat("gdigrab");
                    opt.WithFramerate(30);
                    if (rectangle != null)
                    {
                        opt.WithCustomArgument($"-offset_x {rectangle.Value.X}");
                        opt.WithCustomArgument($"-offset_y {rectangle.Value.Y}");
                        opt.WithCustomArgument($"-video_size {rectangle.Value.Width}x{rectangle.Value.Height}");
                    }
                    WithMuteArgument(opt);
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string path = rectangle != null ? $":0.0+{rectangle.Value.X},{rectangle.Value.Y}" : ":0.0";
                FFMpegArguments = FFMpegArguments.FromFileInput(path, false, opt =>
                {
                    opt.ForceFormat("x11grab");
                    opt.WithFramerate(30);
                    if (rectangle != null)
                    {
                        opt.WithCustomArgument($"-video_size {rectangle.Value.Width}x{rectangle.Value.Height}");
                    }
                    WithMuteArgument(opt);
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                string path = rectangle != null ? $"1:{rectangle.Value.X},{rectangle.Value.Y}" : "1";
                FFMpegArguments = FFMpegArguments.FromFileInput(path, false, opt =>
                {
                    opt.ForceFormat("avfoundation");
                    opt.WithFramerate(30);
                    if (rectangle != null)
                    {
                        opt.WithCustomArgument($"-video_size {rectangle.Value.Width}x{rectangle.Value.Height}");
                    }
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
