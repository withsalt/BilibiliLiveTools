using System.Collections.Generic;
using System.Runtime.InteropServices;
using FlashCap;

namespace BilibiliAutoLiver.Models.Dtos
{
    public class VideoDeviceInfo
    {
        public string Name { get; set; }

        public DeviceTypes DeviceType { get; set; }

        public string Identity { get; set; }

        public List<Characteristics> Characteristics { get; set; }

        public string Description
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return this.Name;
                }
                else
                {
                    if (!string.IsNullOrEmpty(this.Identity) && this.Name != this.Identity)
                    {
                        return $"{this.Name}({this.Identity})";
                    }
                    else
                    {
                        return this.Name;
                    }
                }
            }
        }

        public string DeviceTypeName
        {
            get
            {
                switch (DeviceType)
                {
                    case DeviceTypes.DirectShow:
                        return "dshow";
                    case DeviceTypes.V4L2:
                        return "v4l2";
                    default:
                    case DeviceTypes.VideoForWindows:
                        return "unsupport";
                }
            }
        }
    }

    public class Characteristics
    {
        public int Width { get; set; }

        public int Height { get; set; }

        public PixelFormats Format { get; set; }

        public int Frame { get; set; }

        public string Description
        {
            get
            {
                return this.ToString();
            }
        }

        public override string ToString()
        {
            return $"{Format},{Width}x{Height}@{Frame}";
        }
    }
}
