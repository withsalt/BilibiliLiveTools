using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Models.Settings;
using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Services.FFMpeg.SourceReaders
{
    public class VideoSourceReader : BaseSourceReader
    {
        public VideoSourceReader(LiveSettings settings, string rtmpAddr, ILogger logger) : base(settings, rtmpAddr, logger)
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
            if (string.IsNullOrEmpty(videoSource.Path))
            {
                throw new ArgumentNullException("视频输入源Path不能为空");
            }
            if (!File.Exists(videoSource.Path))
            {
                throw new FileNotFoundException($"视频输入源{videoSource.Path}文件不存在", videoSource.Path);
            }
            var fullPath = Path.GetFullPath(videoSource.Path);
            FFMpegArguments = FFMpegArguments.FromFileInput(fullPath, true, opt =>
            {
                opt.WithCustomArgument("-re");
                opt.WithCustomArgument("-stream_loop -1");

                WithMuteArgument(opt);
            });
        }
    }
}
