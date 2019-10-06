using System;
using System.Collections.Generic;
using System.Text;

namespace BilibiliLiveTools.Model
{
    class LiveSetting
    {
        /// <summary>
        /// 直播类型
        /// </summary>
        public string LiveCategory { get; set; }

        /// <summary>
        /// 直播间名称
        /// </summary>
        public string LiveRoomName { get; set; }

        /// <summary>
        /// 推流命令
        /// </summary>
        public string CmdString { get; set; }

        /// <summary>
        /// 是否自动重启命令
        /// </summary>
        public bool AutoRestart { get; set; }
    }
}
