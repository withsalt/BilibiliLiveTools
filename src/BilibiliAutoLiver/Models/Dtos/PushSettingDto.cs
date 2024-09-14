using System;
using System.Collections.Generic;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Models.Enums;
using FFMpegCore.Enums;

namespace BilibiliAutoLiver.Models.Dtos
{
    public class PushSettingDto : PushSetting
    {
        /// <summary>
        /// 输出分辨率（宽）
        /// </summary>
        public int OutputWidth { get; set; }

        /// <summary>
        /// 输出分辨率（高）
        /// </summary>
        public int OutputHeight { get; set; }

        /// <summary>
        /// 输出分辨率（宽）
        /// </summary>
        public int InputWidth { get; set; }

        /// <summary>
        /// 输出分辨率（高）
        /// </summary>
        public int InputHeight { get; set; }

        /// <summary>
        /// 视频信息
        /// </summary>
        public MaterialDto VideoMaterial { get; set; }

        /// <summary>
        /// 音频信息
        /// </summary>
        public MaterialDto AudioMaterial { get; set; }

        /// <summary>
        /// 所有受支持的编码器
        /// </summary>
        public IReadOnlyList<Codec> VideoCodecs { get; set; }
    }
}
