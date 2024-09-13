using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BilibiliAutoLiver.Models.Dtos;
using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Services.FFMpeg.SourceReaders
{
    public class VideoSourceReader : BaseSourceReader
    {
        public VideoSourceReader(SettingDto setting, string rtmpAddr, ILogger logger) : base(setting, rtmpAddr, logger)
        {

        }

        protected override void GetVideoInputArg()
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
            if (this.Settings.PushSettingDto.VideoInfo.IsDemuxConcat)
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

                    //没有音频的情况下静音视频
                    if (!HasAudioStream() && this.Settings.PushSettingDto.IsMute)
                    {
                        opt.DisableChannel(Channel.Audio);
                    }
                });
            }
            else
            {
                FFMpegArguments = FFMpegArguments.FromFileInput(this.Settings.PushSettingDto.VideoInfo.FullPath, true, opt =>
                {
                    opt.WithCustomArgument("-re");
                    opt.WithCustomArgument("-stream_loop -1");

                    //没有音频的情况下静音视频
                    if (!HasAudioStream() && this.Settings.PushSettingDto.IsMute)
                    {
                        opt.DisableChannel(Channel.Audio);
                    }
                });
            }
        }

        protected override void GetAudioInputArg()
        {
            if (!HasAudioStream())
            {
                return;
            }
            this.FFMpegArguments.AddFileInput(this.Settings.PushSettingDto.AudioInfo.FullPath, true, opt =>
            {
                opt.WithCustomArgument("-stream_loop -1");
            });
        }

        protected override void WithAudioArgument(FFMpegArgumentOptions opt)
        {
            if (this.Settings.PushSettingDto.VideoInfo.IsDemuxConcat)
            {
                return;
            }
            if (!HasAudioStream())
            {
                if (!this.Settings.PushSettingDto.IsMute && this.Settings.PushSettingDto.VideoInfo.MediaInfo.AudioStream != null)
                {
                    base.WithAudioArgument(opt);
                }
                return;
            }
            else
            {
                if (this.Settings.PushSettingDto.IsMute)
                {
                    //视频静音，但是包含音频
                    opt.WithCustomArgument($"-map 0:v:{this.Settings.PushSettingDto.VideoInfo.MediaInfo.PrimaryIndex}");
                    opt.WithCustomArgument($"-map 1:a:{this.Settings.PushSettingDto.AudioInfo.MediaInfo.PrimaryIndex}");
                }
                else
                {
                    //视频不静音，但是包含音频
                    if (this.Settings.PushSettingDto.VideoInfo.MediaInfo.AudioStream == null)
                    {
                        //视频不包含音轨
                        opt.WithCustomArgument($"-map 0:v:{this.Settings.PushSettingDto.VideoInfo.MediaInfo.PrimaryIndex}");
                        opt.WithCustomArgument($"-map 1:a:{this.Settings.PushSettingDto.AudioInfo.MediaInfo.PrimaryIndex}");
                    }
                    else
                    {
                        opt.WithCustomArgument($"-filter_complex \"[0:a:0][1:a:{this.Settings.PushSettingDto.AudioInfo.MediaInfo.PrimaryIndex}]amerge=inputs=2[aout]\" -map 0:v:{this.Settings.PushSettingDto.VideoInfo.MediaInfo.PrimaryIndex} -map \"[aout]\"");
                    }
                }
                base.WithAudioArgument(opt);
            }
        }

        protected override bool HasAudioStream()
        {
            if (this.Settings.PushSettingDto.VideoInfo.IsDemuxConcat)
            {
                return false;
            }
            if (this.Settings.PushSettingDto.AudioInfo == null || string.IsNullOrWhiteSpace(this.Settings.PushSettingDto.AudioInfo.FullPath))
            {
                return false;
            }
            if (!File.Exists(this.Settings.PushSettingDto.AudioInfo.FullPath))
            {
                throw new FileNotFoundException($"音频输入源{this.Settings.PushSettingDto.AudioInfo.FullPath}文件不存在", this.Settings.PushSettingDto.AudioInfo.FullPath);
            }
            return true;
        }
    }
}
