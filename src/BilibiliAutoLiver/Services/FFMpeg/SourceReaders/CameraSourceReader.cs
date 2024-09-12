using System;
using System.Runtime.InteropServices;
using BilibiliAutoLiver.Models.Dtos;
using FFMpegCore;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Services.FFMpeg.SourceReaders
{
    public class CameraSourceReader : BaseSourceReader
    {
        public CameraSourceReader(SettingDto setting, string rtmpAddr, ILogger logger) : base(setting, rtmpAddr, logger)
        {

        }

        protected override void GetAudioInputArg()
        {
            throw new NotImplementedException();
        }

        protected override void GetVideoInputArg()
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

        protected override bool HasAudioStream()
        {
            throw new NotImplementedException();
        }
    }
}
