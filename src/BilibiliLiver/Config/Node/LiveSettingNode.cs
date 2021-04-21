using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLiver.Config
{
    public class LiveSettingNode
    {
        public string LiveCategory { get; set; }

        public string LiveRoomName { get; set; }

        public string FFmpegCmd { get; set; }

        public bool AutoRestart { get; set; }
    }
}
