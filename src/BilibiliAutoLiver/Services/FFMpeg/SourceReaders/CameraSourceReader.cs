using System;
using System.Runtime.InteropServices;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Models.Settings;
using FFMpegCore;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Services.FFMpeg.SourceReaders
{
    public class CameraSourceReader : BaseSourceReader
    {
        public CameraSourceReader(SettingDto setting, string rtmpAddr, ILogger logger) : base(setting, rtmpAddr, logger)
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
            PushSettingDto pushSetting = this.Settings.PushSettingDto;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                FFMpegArguments = FFMpegArguments.FromFileInput($"video={pushSetting.DeviceName}", false, opt =>
                {
                    opt.ForceFormat("dshow");
                    opt.WithFramerate(pushSetting.InputFramerate);
                    opt.WithCustomArgument($"-video_size {pushSetting.InputWidth}x{pushSetting.InputHeight}");
                    WithMuteArgument(opt);
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                FFMpegArguments = FFMpegArguments.FromFileInput($"video={pushSetting.DeviceName}", false, opt =>
                {
                    opt.ForceFormat("v4l2");
                    opt.WithFramerate(pushSetting.InputFramerate);
                    opt.WithCustomArgument($"-video_size {pushSetting.InputWidth}x{pushSetting.InputHeight}");
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
