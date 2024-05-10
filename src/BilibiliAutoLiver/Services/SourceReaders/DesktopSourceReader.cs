using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using BilibiliAutoLiver.Models;
using BilibiliAutoLiver.Models.Enums;
using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Services.SourceReaders
{
    public class DesktopSourceReader : BaseSourceReader
    {
        public DesktopSourceReader(LiveSettings settings, string rtmpAddr, ILogger logger) : base(settings, rtmpAddr, logger)
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
            if (this.FFMpegArguments == null) throw new Exception("请先指定输入参数");
            var rt = this.FFMpegArguments.OutputToUrl(this.RtmpAddr, opt =>
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
            InputVideoSource videoSource = this.Settings.V2.Input.VideoSource;
            Rectangle? rectangle = AnalyzeRectangle();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                this.FFMpegArguments = FFMpegArguments.FromFileInput("desktop", false, opt =>
                {
                    opt.ForceFormat("gdigrab");
                    opt.WithFramerate(30);
                    if (rectangle != null)
                    {
                        opt.WithCustomArgument($"-offset_x {rectangle.Value.X}");
                        opt.WithCustomArgument($"-offset_y {rectangle.Value.Y}");
                        opt.WithCustomArgument($"-video_size {rectangle.Value.Width}x{rectangle.Value.Height}");
                    }
                    WithMuteArgument(videoSource, opt);
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string path = rectangle != null ? $":0.0+{rectangle.Value.X},{rectangle.Value.Y}" : ":0.0";
                this.FFMpegArguments = FFMpegArguments.FromFileInput(path, false, opt =>
                {
                    opt.ForceFormat("x11grab");
                    opt.WithFramerate(30);
                    if (rectangle != null)
                    {
                        opt.WithCustomArgument($"-video_size {rectangle.Value.Width}x{rectangle.Value.Height}");
                    }
                    WithMuteArgument(videoSource, opt);
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                string path = rectangle != null ? $"1:{rectangle.Value.X},{rectangle.Value.Y}" : "1";
                this.FFMpegArguments = FFMpegArguments.FromFileInput(path, false, opt =>
                {
                    opt.ForceFormat("avfoundation");
                    opt.WithFramerate(30);
                    if (rectangle != null)
                    {
                        opt.WithCustomArgument($"-video_size {rectangle.Value.Width}x{rectangle.Value.Height}");
                    }
                    WithMuteArgument(videoSource, opt);
                });
            }
            else
            {
                throw new NotSupportedException("不支持的系统类型");
            }
        }

        private void WithMuteArgument(InputVideoSource videoSource, FFMpegArgumentOptions opt)
        {
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
        }

        private Rectangle? AnalyzeRectangle()
        {
            if (string.IsNullOrWhiteSpace(this.Settings.V2.Input.VideoSource.Path))
            {
                return null;
            }
            string[] param = this.Settings.V2.Input.VideoSource.Path.Split(',');
            if (param == null || param.Length != 4)
            {
                _logger.LogInformation("Path参数不正确，示例：0,0,800,800（x, y, width, height）");
                return null;
            }
            int[] paramVal = param.Select(p => int.TryParse(p, out int oVal) ? oVal : -1).ToArray();
            if (paramVal[0] < 0)
            {
                _logger.LogInformation("x参数不正确，x坐标不能小于0，示例：0,0,800,800（x, y, width, height）");
                return null;
            }
            if (paramVal[1] < 0)
            {
                _logger.LogInformation("y参数不正确，y坐标不能小于0，示例：0,0,800,800（x, y, width, height）");
                return null;
            }
            if (paramVal[2] < 0)
            {
                _logger.LogInformation("width参数不正确，width坐标不能小于0，示例：0,0,800,800（x, y, width, height）");
                return null;
            }
            if (paramVal[3] < 0)
            {
                _logger.LogInformation("height参数不正确，height坐标不能小于0，示例：0,0,800,800（x, y, width, height）");
                return null;
            }
            return new Rectangle(paramVal[0], paramVal[1], paramVal[2], paramVal[3]);
        }
    }
}
