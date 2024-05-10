using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BilibiliAutoLiver.Models;
using BilibiliAutoLiver.Models.Enums;
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
            if (HasAudio())
            {
                GetAudioInputArg(Settings.V2.Input.AudioSource);
            }
            return this;
        }

        public override FFMpegArgumentProcessor WithOutputArg()
        {
            if (FFMpegArguments == null) throw new Exception("请先指定输入参数");
            var rt = FFMpegArguments.OutputToUrl(RtmpAddr, opt =>
            {
                //禁用视频中的音频
                if (!string.IsNullOrEmpty(videoMuteMapOpt))
                {
                    opt.WithCustomArgument(videoMuteMapOpt);
                }
                //禁用音频中的视频
                if (!string.IsNullOrEmpty(audioMuteMapOpt))
                {
                    opt.WithCustomArgument(audioMuteMapOpt);
                }
                //音频编码
                if (HasAudio())
                {
                    opt.WithAudioCodec(AudioCodec.Aac);
                }
                //视频编码
                opt.WithVideoCodec(VideoCodec.LibX264);
                opt.ForceFormat("flv");
                opt.ForcePixelFormat("yuv420p");
                opt.WithConstantRateFactor(20);
                opt.UsingShortest();
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
        }

        private void GetAudioInputArg(InputAudioSource audioSource)
        {
            if (!HasAudio())
            {
                return;
            }
            var fullPath = Path.GetFullPath(audioSource.Path);
            FFMpegArguments.AddFileInput(fullPath, true, opt =>
            {
                opt.WithCustomArgument("-stream_loop -1");
                audioMuteMapOpt = "-map 1:a:0";
            });
        }
    }
}
