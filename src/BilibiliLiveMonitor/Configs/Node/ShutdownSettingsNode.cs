using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLiveMonitor.Configs
{
    public class ShutdownSettingsNode
    {
        public bool IsEnableShutdown { get; set; }

        public int RequestFailedCount { get; set; }

        public double BatteryDown { get; set; }

        public static string Position { get { return "ShutdownSettings"; } }
    }
}
