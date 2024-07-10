using System.Collections.Generic;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Models.Entities;

namespace BilibiliAutoLiver.Models.ViewModels
{
    public class PushIndexPageViewModel
    {
        public PushSetting PushSetting { get; set; }

        public string ListDeviceFFmpegCmd { get; set; }

        /// <summary>
        /// 视频素材
        /// </summary>
        public Dictionary<long, string> Videos { get; set; } = new Dictionary<long, string>();

        /// <summary>
        /// 音频素材
        /// </summary>
        public Dictionary<long, string> Audios { get; set; } = new Dictionary<long, string>();
    }
}
