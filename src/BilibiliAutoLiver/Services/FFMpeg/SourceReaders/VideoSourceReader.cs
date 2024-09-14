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
            if (this.Settings.PushSetting.VideoMaterial == null
                || string.IsNullOrEmpty(this.Settings.PushSetting.VideoMaterial.FullPath))
            {
                throw new ArgumentNullException("视频输入源Path不能为空");
            }
            if (!File.Exists(this.Settings.PushSetting.VideoMaterial.FullPath))
            {
                throw new FileNotFoundException($"视频输入源{this.Settings.PushSetting.VideoMaterial.FullPath}文件不存在", this.Settings.PushSetting.VideoMaterial.FullPath);
            }
            if (this.Settings.PushSetting.VideoMaterial.IsDemuxConcat)
            {
                var allLines = System.IO.File.ReadAllLines(this.Settings.PushSetting.VideoMaterial.FullPath)
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
                    if (!HasAudioStream() && this.Settings.PushSetting.IsMute)
                    {
                        opt.DisableChannel(Channel.Audio);
                    }
                });
            }
            else
            {
                FFMpegArguments = FFMpegArguments.FromFileInput(this.Settings.PushSetting.VideoMaterial.FullPath, true, opt =>
                {
                    opt.WithCustomArgument("-re");
                    opt.WithCustomArgument("-stream_loop -1");

                    //没有音频的情况下静音视频
                    if (!HasAudioStream() && this.Settings.PushSetting.IsMute)
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
            this.FFMpegArguments.AddFileInput(this.Settings.PushSetting.AudioMaterial.FullPath, true, opt =>
            {
                opt.WithCustomArgument("-stream_loop -1");
            });
        }

        protected override void GetAudioOutputArg(FFMpegArgumentOptions opt)
        {
            if (this.Settings.PushSetting.VideoMaterial.IsDemuxConcat)
            {
                return;
            }
            if (!HasAudioStream())
            {
                if (!this.Settings.PushSetting.IsMute && this.Settings.PushSetting.VideoMaterial.MediaInfo.AudioStream != null)
                {
                    base.GetAudioOutputArg(opt);
                }
                return;
            }
            else
            {
                if (this.Settings.PushSetting.IsMute)
                {
                    //视频静音，但是包含音频
                    opt.WithCustomArgument($"-map 0:v:{this.Settings.PushSetting.VideoMaterial.MediaInfo.PrimaryIndex}");
                    opt.WithCustomArgument($"-map 1:a:{this.Settings.PushSetting.AudioMaterial.MediaInfo.PrimaryIndex}");
                }
                else
                {
                    //视频不静音，但是包含音频
                    if (this.Settings.PushSetting.VideoMaterial.MediaInfo.AudioStream == null)
                    {
                        //视频不包含音轨
                        opt.WithCustomArgument($"-map 0:v:{this.Settings.PushSetting.VideoMaterial.MediaInfo.PrimaryIndex}");
                        opt.WithCustomArgument($"-map 1:a:{this.Settings.PushSetting.AudioMaterial.MediaInfo.PrimaryIndex}");
                    }
                    else
                    {
                        opt.WithCustomArgument($"-filter_complex \"[0:a:0][1:a:{this.Settings.PushSetting.AudioMaterial.MediaInfo.PrimaryIndex}]amerge=inputs=2[aout]\" -map 0:v:{this.Settings.PushSetting.VideoMaterial.MediaInfo.PrimaryIndex} -map \"[aout]\"");
                    }
                }
                base.GetAudioOutputArg(opt);
            }
        }

        protected override bool HasAudioStream()
        {
            if (this.Settings.PushSetting.VideoMaterial.IsDemuxConcat)
            {
                return false;
            }
            if (this.Settings.PushSetting.AudioMaterial == null || string.IsNullOrWhiteSpace(this.Settings.PushSetting.AudioMaterial.FullPath))
            {
                return false;
            }
            if (!File.Exists(this.Settings.PushSetting.AudioMaterial.FullPath))
            {
                throw new FileNotFoundException($"音频输入源{this.Settings.PushSetting.AudioMaterial.FullPath}文件不存在", this.Settings.PushSetting.AudioMaterial.FullPath);
            }
            return true;
        }
    }
}
