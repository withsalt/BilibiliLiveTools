using System;
using System.Collections.Generic;
using BilibiliAutoLiver.Models.Dtos;
using FFMpegCore;
using FFMpegCore.Enums;

namespace BilibiliAutoLiver.Services.FFMpeg.Ext
{
    public static class FFMpegArgumentOptionsExtension
    {
        private static HashSet<string> CodecNames = new HashSet<string>()
        {
            "libx264", "libx265"
        };

        public static void Withx264Orx265SpeedPreset(this FFMpegArgumentOptions opt, Codec codec, string speedPreset)
        {
            if (CodecNames.Contains(codec.Name) && !string.IsNullOrWhiteSpace(speedPreset) && Enum.IsDefined(typeof(Speed), speedPreset) && Enum.TryParse<Speed>(speedPreset, out var speed))
            {
                opt.WithSpeedPreset(speed);
            }
        }

        public static void WithLowDelayArgument(this FFMpegArgumentOptions opt)
        {
            opt.WithCustomArgument("-reorder_queue_size 0");
            opt.WithCustomArgument("-flags low_delay");
        }

        public static void WithSettingsVideoInputArgument(this FFMpegArgumentOptions opt, SettingDto settings)
        {
            if (!string.IsNullOrWhiteSpace(settings?.AppSettings?.FFmpegPresetParams?.InputQuality?.VideoCustomArgument))
            {
                opt.WithCustomArgument(settings.AppSettings.FFmpegPresetParams.InputQuality.VideoCustomArgument);
            }
        }

        public static void WithSettingsAudioInputArgument(this FFMpegArgumentOptions opt, SettingDto settings)
        {
            if (!string.IsNullOrWhiteSpace(settings?.AppSettings?.FFmpegPresetParams?.InputQuality?.AudioCustomArgument))
            {
                opt.WithCustomArgument(settings.AppSettings.FFmpegPresetParams.InputQuality.AudioCustomArgument);
            }
        }
    }
}
