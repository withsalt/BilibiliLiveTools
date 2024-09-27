using System.Collections.Generic;
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

        public static void Withx264Orx265SpeedPreset(this FFMpegArgumentOptions opt, Codec codec)
        {
            if (CodecNames.Contains(codec.Name))
            {
                opt.WithSpeedPreset(Speed.UltraFast);
            }
        }

        public static void WithLowDelayArgument(this FFMpegArgumentOptions opt)
        {
            opt.WithCustomArgument("-reorder_queue_size 0");
            opt.WithCustomArgument("-flags low_delay");
        }
    }
}
