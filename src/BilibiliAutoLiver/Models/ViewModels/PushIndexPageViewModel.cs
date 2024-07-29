using System.Collections.Generic;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Models.Entities;

namespace BilibiliAutoLiver.Models.ViewModels
{
    public class PushIndexPageViewModel
    {
        public PushSetting PushSetting { get; set; }

        /// <summary>
        /// 支持的视频设备
        /// </summary>
        public List<string> VideoDevices { get; set; }

        /// <summary>
        /// 支持的音频设备
        /// </summary>
        public List<string> AudioDevices { get; set; }

        /// <summary>
        /// 视频素材
        /// </summary>
        public Dictionary<long, string> Videos { get; set; } = new Dictionary<long, string>();

        /// <summary>
        /// 音频素材
        /// </summary>
        public Dictionary<long, string> Audios { get; set; } = new Dictionary<long, string>();

        /// <summary>
        /// 插件
        /// </summary>
        public List<string> Plugins { get; set; }
    }
}
