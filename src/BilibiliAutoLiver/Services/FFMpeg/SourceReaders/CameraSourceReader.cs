using System;
using System.IO;
using System.Runtime.InteropServices;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Utils;
using FFMpegCore;
using FFMpegCore.Enums;
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
                        opt.ForceFormat(format);
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    FFMpegArguments = FFMpegArguments.AddDeviceInput($"audio=\"{deviceName}\"", opt =>
                    {
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
                    opt.WithCustomArgument("-stream_loop -1");
                });
                return;
            }
            throw new NotSupportedException("未知的音频输入类型");
        }

        protected override void GetVideoInputArg()
        {
            PushSettingDto pushSetting = this.Settings.PushSetting;
            (string format, string deviceName) = CommonHelper.GetDeviceFormatAndName(this.Settings.PushSetting.DeviceName);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                FFMpegArguments = FFMpegArguments.FromDeviceInput($"video=\"{deviceName}\"", opt =>
                {
                    opt.ForceFormat(format);
                    opt.WithFramerate(pushSetting.InputFramerate);
                    opt.WithCustomArgument($"-video_size {pushSetting.InputWidth}x{pushSetting.InputHeight}");
                    //没有音频的情况下静音视频
                    if (!HasAudioStream())
                    {
                        opt.DisableChannel(Channel.Audio);
                    }
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                FFMpegArguments = FFMpegArguments.FromDeviceInput($"video=\"{deviceName}\"", opt =>
                {
                    opt.ForceFormat(format);
                    opt.WithFramerate(pushSetting.InputFramerate);
                    opt.WithCustomArgument($"-video_size {pushSetting.InputWidth}x{pushSetting.InputHeight}");
                    //没有音频的情况下静音视频
                    if (!HasAudioStream())
                    {
                        opt.DisableChannel(Channel.Audio);
                    }
                });
            }
            else
            {
                throw new NotSupportedException("不支持的系统类型");
            }
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
