using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLiveMonitor.Configs
{
    public class LiveSettingsNode
    {
        public int RoomId { get; set; }

        public static string Position { get { return "LiveSettings"; } }

    }
}
