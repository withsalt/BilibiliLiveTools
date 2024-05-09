using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BilibiliAutoLiver.Models;
using BilibiliAutoLiver.Models.Enums;
using FFMpegCore;
using FFMpegCore.Enums;

namespace BilibiliAutoLiver.Services.SourceReaders
{
    public class VideoSourceReader : BaseSourceReader
    {
        public VideoSourceReader(LiveSettings settings) : base(settings)
        {

        }

        public override FFMpegArguments BuildInputArg()
        {
            FFMpegArguments arg = null;
            switch (this.Settings.V2.Input.VideoSource.Type)
            {
                case InputSourceType.File:
                    arg = GetVideoInputArg(this.Settings.V2.Input.VideoSource);
                    break;
                case InputSourceType.Device:
                    break;
                default:
                    throw new Exception("为知视频输入类型。");

            }

            if (this.Settings.V2.Input.AudioSource != null)
            {
                arg = GetAudioInputArg(arg, this.Settings.V2.Input.AudioSource);
            }
            return arg;
        }

        private FFMpegArguments GetDeviceInputArg(InputVideoSource[] deviceSources)
        {

            return null;
        }

        private FFMpegArguments GetVideoInputArg(InputVideoSource videoSource)
        {
            if (string.IsNullOrEmpty(videoSource.Path))
            {
                throw new ArgumentNullException("视频输入源Path不能为空");
            }
            if (!File.Exists(videoSource.Path))
            {
                throw new FileNotFoundException($"视频输入源{videoSource.Path}文件不存在", videoSource.Path);
            }
            var fullPath = Path.GetFullPath(videoSource.Path);
            var arg = FFMpegArguments.FromFileInput(fullPath, true, opt =>
            {
                opt.WithCustomArgument("-re");
                opt.WithCustomArgument("-stream_loop -1");
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
            });
            return arg;
        }

        private FFMpegArguments GetAudioInputArg(FFMpegArguments arg, InputAudioSource audioSource)
        {
            if (!HasAudio())
            {
                return arg;
            }
            var fullPath = Path.GetFullPath(audioSource.Path);
            arg.AddFileInput(fullPath, true, opt =>
            {
                opt.WithCustomArgument("-stream_loop -1");
                audioMuteMapOpt = "-map 1:a:0";
            });
            return arg;
        }
    }
}
