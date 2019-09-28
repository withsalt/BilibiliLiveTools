using System;
using System.Collections.Generic;
using System.Text;

namespace BilibiliRtmpPublisher.Model
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
        /// 视频来源
        /// </summary>
        public string VideoSource { get; set; }

        /// <summary>
        /// 音频来源
        /// </summary>
        public string AudioSource { get; set; }

        /// <summary>
        /// 分辨率
        /// </summary>
        public string Resolution { get; set; }
    }
}
