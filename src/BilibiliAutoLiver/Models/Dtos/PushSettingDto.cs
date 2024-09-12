using BilibiliAutoLiver.Models.Enums;

namespace BilibiliAutoLiver.Models.Dtos
{
    public class PushSettingDto
    {
        /// <summary>
        /// 输入类型
        /// </summary>
        public InputType InputType { get; set; }

        /// <summary>
        /// 输出质量
        /// </summary>
        public OutputQualityEnum Quality { get; set; }

        /// <summary>
        /// 输出分辨率（宽）
        /// </summary>
        public int OutputWidth { get; set; }

        /// <summary>
        /// 输出分辨率（高）
        /// </summary>
        public int OutputHeight { get; set; }

        /// <summary>
        /// 自定义输出参数
        /// </summary>
        public string CustumOutputParams { get; set; }

        /// <summary>
        /// 是否静音
        /// </summary>
        public bool IsMute { get; set; }

        /// <summary>
        /// 输入屏幕参数
        /// </summary>
        public string InputScreen { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// 输出分辨率（宽）
        /// </summary>
        public int InputWidth { get; set; }

        /// <summary>
        /// 输出分辨率（高）
        /// </summary>
        public int InputHeight { get; set; }

        /// <summary>
        /// 帧数
        /// </summary>
        public double InputFramerate { get; set; }

        /// <summary>
        /// 视频信息
        /// </summary>
        public MaterialDto VideoInfo { get; set; }

        /// <summary>
        /// 音频信息
        /// </summary>
        public MaterialDto AudioInfo { get; set; }
    }
}
