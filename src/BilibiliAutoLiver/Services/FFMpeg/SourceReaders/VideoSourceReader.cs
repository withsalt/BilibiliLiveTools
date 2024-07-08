using System;
using System.IO;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Models.Settings;
using FFMpegCore;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Services.FFMpeg.SourceReaders
{
    public class VideoSourceReader : BaseSourceReader
    {
        public VideoSourceReader(SettingDto setting, string rtmpAddr, ILogger logger) : base(setting, rtmpAddr, logger)
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
            if (string.IsNullOrEmpty(this.Settings.PushSettingDto.VideoPath))
            {
                throw new ArgumentNullException("视频输入源Path不能为空");
            }
            if (!File.Exists(this.Settings.PushSettingDto.VideoPath))
            {
                throw new FileNotFoundException($"视频输入源{this.Settings.PushSettingDto.VideoPath}文件不存在", this.Settings.PushSettingDto.VideoPath);
            }
            FFMpegArguments = FFMpegArguments.FromFileInput(this.Settings.PushSettingDto.VideoPath, true, opt =>
            {
                opt.WithCustomArgument("-re");
                opt.WithCustomArgument("-stream_loop -1");

                WithMuteArgument(opt);
            });
        }
    }
}
