namespace BilibiliLiver.Config.Models
{
    public class LiveSettings
    {
        public int LiveAreaId { get; set; }

        public string LiveRoomName { get; set; }

        public string FFmpegCmd { get; set; }

        public bool AutoRestart { get; set; }

        public int RepushFailedExitMinutes { get; set; }

        public static string Position { get { return "LiveSettings"; } }
    }
}
