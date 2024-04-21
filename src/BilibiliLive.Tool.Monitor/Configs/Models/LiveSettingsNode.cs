using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Tool.Monitor.Configs.Models
{
    public class LiveSettingsNode
    {
        public long RoomId { get; set; }

        public static string Position { get { return "LiveSettings"; } }

    }
}
