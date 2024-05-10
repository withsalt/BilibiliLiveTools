using System;
using System.Runtime.InteropServices;
using BilibiliAutoLiver.Models.Enums;

namespace BilibiliAutoLiver.Models
{
    public class LiveSettings
    {
        public int LiveAreaId { get; set; }

        public string LiveRoomName { get; set; }

        private int _retryDelay { get; set; }
        public int RetryDelay
        {
            get
            {
                return _retryDelay;
            }
            set
            {
                if (value < 10)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "重试等待时间不能小于10s");
                }
                _retryDelay = value;
            }
        }

        public PushStreamV1Config V1 { get; set; }

        public PushStreamV2Config V2 { get; set; }

        public static string Position { get { return "LiveSettings"; } }
    }

    public abstract class BasePushStreamConfig
    {
        public bool IsEnabled { get; set; }
    }

    public class PushStreamV1Config : BasePushStreamConfig
    {
        public FFmpegCommands FFmpegCommands { get; set; }
    }

    public class PushStreamV2Config : BasePushStreamConfig
    {
        public Output Output { get; set; }

        public Input Input { get; set; }
    }

    public class FFmpegCommands
    {
        public string Win { get; set; }

        public string Linux { get; set; }

        /// <summary>
        /// 获取对应平台的ffmpeg命令
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        public string GetTargetOSPlatformCommand()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return this.Win;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return this.Linux;
            }
            throw new PlatformNotSupportedException("Not support your OS platform.");
        }
    }

    public class Output
    {
        public OutputQualityEnum Quality { get; set; }

        public string Resolution { get; set; }
    }

    public class Input
    {
        public InputVideoSource VideoSource { get; set; }

        public InputAudioSource AudioSource { get; set; }
    }
}
