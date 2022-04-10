namespace BilibiliLiver.Config.Models
{
    public class LiveSetting
    {
        public string LiveCategory { get; set; }

        public string LiveRoomName { get; set; }

        public string FFmpegCmd { get; set; }

        public bool AutoRestart { get; set; }

        public static string Position { get { return "LiveSetting"; } }
    }
}
