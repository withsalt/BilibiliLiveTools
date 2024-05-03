using System;
using System.Runtime.InteropServices;
using BilibiliAutoLiver.Models.Enums;

namespace BilibiliAutoLiver.Models
{
    public class LiveSettings
    {
        public int LiveAreaId { get; set; }

        public string LiveRoomName { get; set; }

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
}
