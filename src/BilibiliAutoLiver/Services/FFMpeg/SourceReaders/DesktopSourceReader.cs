using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Services.FFMpeg.Extension;
using BilibiliAutoLiver.Utils;
using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace BilibiliAutoLiver.Services.FFMpeg.SourceReaders
{
    public class DesktopSourceReader : BaseSourceReader
    {
        public DesktopSourceReader(SettingDto setting, string rtmpAddr, ILogger logger) : base(setting, rtmpAddr, logger)
        {

        }

        protected override void GetAudioInputArg()
        {
            if (!HasAudioStream())
            {
                return;
            }
            if (!string.IsNullOrEmpty(this.Settings.PushSetting.AudioDevice))
            {
                (string format, string deviceName) = CommonHelper.GetDeviceFormatAndName(this.Settings.PushSetting.AudioDevice);
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    FFMpegArguments = FFMpegArguments.AddDeviceInput($"audio=\"{deviceName}\"", opt =>
                    {
                        opt.WithSettingsAudioInputArgument(this.Settings);
                        opt.ForceFormat(format);
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    FFMpegArguments = FFMpegArguments.AddDeviceInput($"\"{deviceName}\"", opt =>
                    {
                        opt.WithSettingsAudioInputArgument(this.Settings);
                        opt.ForceFormat(format);
                    });
                }
                else
                {
                    throw new NotSupportedException("不支持的系统类型");
                }
                return;
            }
            else if (this.Settings.PushSetting.AudioMaterial != null && !string.IsNullOrWhiteSpace(this.Settings.PushSetting.AudioMaterial.FullPath))
            {
                this.FFMpegArguments.AddFileInput(this.Settings.PushSetting.AudioMaterial.FullPath, true, opt =>
                {
                    opt.WithSettingsAudioInputArgument(this.Settings);
                    opt.WithCustomArgument("-stream_loop -1");
                });
                return;
            }
            throw new NotSupportedException("未知的音频输入类型");
        }

        protected override void GetVideoInputArg()
        {
            if (!ScreenParamsHelper.TryParse(this.Settings.PushSetting.InputScreen, out string message, out SKRect? rectangle))
            {
                throw new Exception(message);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                FFMpegArguments = FFMpegArguments.FromFileInput("desktop", false, opt =>
                {
                    opt.WithSettingsVideoInputArgument(this.Settings);
                    opt.ForceFormat("gdigrab");
                    opt.WithFramerate(30);
                    if (rectangle != null)
                    {
                        opt.WithCustomArgument($"-offset_x {rectangle.Value.Location.X}");
                        opt.WithCustomArgument($"-offset_y {rectangle.Value.Location.Y}");
                        if (rectangle.Value.Size.Width > 0 && rectangle.Value.Size.Height > 0)
                        {
                            opt.WithCustomArgument($"-video_size {rectangle.Value.Size.Width}x{rectangle.Value.Size.Height}");
                        }
                    }
                    //没有音频的情况下静音视频
                    if (!HasAudioStream())
                    {
                        opt.DisableChannel(Channel.Audio);
                    }
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string path = rectangle != null ? $":0.0+{rectangle.Value.Location.X},{rectangle.Value.Location.Y}" : ":0.0";
                FFMpegArguments = FFMpegArguments.FromFileInput(path, false, opt =>
                {
                    opt.WithSettingsVideoInputArgument(this.Settings);
                    opt.ForceFormat("x11grab");
                    opt.WithFramerate(30);
                    if (rectangle != null && rectangle.Value.Size.Width > 0 && rectangle.Value.Size.Height > 0)
                    {
                        opt.WithCustomArgument($"-video_size {rectangle.Value.Size.Width}x{rectangle.Value.Size.Height}");
                    }
                    //没有音频的情况下静音视频
                    if (!HasAudioStream())
                    {
                        opt.DisableChannel(Channel.Audio);
                    }
                });
            }
            //else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            //{
            //    string path = rectangle != null ? $"1:{rectangle.Value.X},{rectangle.Value.Y}" : "1";
            //    FFMpegArguments = FFMpegArguments.FromFileInput(path, false, opt =>
            //    {
            //        opt.ForceFormat("avfoundation");
            //        opt.WithFramerate(30);
            //        if (rectangle != null)
            //        {
            //            opt.WithCustomArgument($"-video_size {rectangle.Value.Width}x{rectangle.Value.Height}");
            //        }
            //        //没有音频的情况下静音视频
            //        if (!HasAudioStream())
            //        {
            //            opt.DisableChannel(Channel.Audio);
            //        }
            //    });
            //}
            else
            {
                throw new NotSupportedException("不支持的系统类型");
            }
        }

        protected override void GetAudioOutputArg(FFMpegArgumentOptions opt)
        {
            if (!HasAudioStream())
            {
                return;
            }
            base.GetAudioOutputArg(opt);
        }

        protected override bool HasAudioStream()
        {
            if (!string.IsNullOrEmpty(this.Settings.PushSetting.AudioDevice))
            {
                return true;
            }
            if (this.Settings.PushSetting.AudioMaterial != null && !string.IsNullOrWhiteSpace(this.Settings.PushSetting.AudioMaterial.FullPath))
            {
                if (!File.Exists(this.Settings.PushSetting.AudioMaterial.FullPath))
                {
                    throw new FileNotFoundException($"音频输入源{this.Settings.PushSetting.AudioMaterial.FullPath}文件不存在", this.Settings.PushSetting.AudioMaterial.FullPath);
                }
                return true;
            }
            return false;
        }
    }
}
