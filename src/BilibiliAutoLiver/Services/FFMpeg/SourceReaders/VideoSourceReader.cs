using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BilibiliAutoLiver.Models.Dtos;
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
            if (this.Settings.PushSettingDto.VideoInfo == null 
                || string.IsNullOrEmpty(this.Settings.PushSettingDto.VideoInfo.FullPath))
            {
                throw new ArgumentNullException("视频输入源Path不能为空");
            }
            if (!File.Exists(this.Settings.PushSettingDto.VideoInfo.FullPath))
            {
                throw new FileNotFoundException($"视频输入源{this.Settings.PushSettingDto.VideoInfo.FullPath}文件不存在", this.Settings.PushSettingDto.VideoInfo.FullPath);
            }
            if (this.Settings.PushSettingDto.VideoInfo.FullPath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                var allLines = System.IO.File.ReadAllLines(this.Settings.PushSettingDto.VideoInfo.FullPath)
                    ?.Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select(p => p.Trim(' ', '\r', '\n'))
                    .ToList();
                if (allLines?.Any() != true)
                {
                    throw new Exception("上传的素材列表文件为空");
                }
                List<string> allFiles = new List<string>();
                foreach (var item in allLines)
                {
                    if (item.StartsWith("file ", StringComparison.OrdinalIgnoreCase))
                    {
                        allFiles.Add(item.Substring(5).Trim(' ', '\r', '\n', '\'', '\"'));
                    }
                    else
                    {
                        allFiles.Add(item);
                    }
                }

                FFMpegArguments = FFMpegArguments.FromDemuxConcatInput(allFiles, opt =>
                {
                    opt.WithCustomArgument("-re");
                    opt.WithCustomArgument("-stream_loop -1");

                    WithMuteArgument(opt);
                });
            }
            else
            {
                FFMpegArguments = FFMpegArguments.FromFileInput(this.Settings.PushSettingDto.VideoInfo.FullPath, true, opt =>
                {
                    opt.WithCustomArgument("-re");
                    opt.WithCustomArgument("-stream_loop -1");

                    WithMuteArgument(opt);
                });
            }
        }
    }
}
