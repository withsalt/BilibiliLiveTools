using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLive.Tool.Monitor.Configs.Models
{
    public class AppSettingsNode
    {
        public int IntervalTime { get; set; }

        public bool IsEnableEmailNotice { get; set; }

        public EmailConfigNode EmailConfig { get; set; }

        public static string Position { get { return "AppSettings"; } }

    }
}
