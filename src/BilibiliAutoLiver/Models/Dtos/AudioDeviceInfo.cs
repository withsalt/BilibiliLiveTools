using System.Collections.Generic;
using System.Runtime.InteropServices;
using FlashCap;

namespace BilibiliAutoLiver.Models.Dtos
{
    public class AudioDeviceInfo
    {
        public string Name { get; set; }

        public string DeviceName { get; set; }

        public AudioDeviceType DeviceType { get; set; }

        public int CardIndex { get; set; }

        public int DeviceIndex { get; set; }

        public string Identity
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return this.Name;
                }
                else
                {
                    return $"hw:{this.CardIndex},{this.DeviceIndex}";
                }
            }
        }

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
                    case AudioDeviceType.DirectShow:
                        return "dshow";
                    case AudioDeviceType.Alsa:
                        return "alsa";
                    default:
                        return "unsupport";
                }
            }
        }
    }

    public enum AudioDeviceType
    {
        DirectShow = 1,

        Alsa = 2,
    }
}
