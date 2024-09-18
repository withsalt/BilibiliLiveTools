namespace BilibiliAutoLiver.Models.Settings
{
    public class FFMpegPresetParams
    {
        public static string Position { get { return "FFMpegPresetParams"; } }

        /// <summary>
        /// 获取或设置像素格式
        /// </summary>
        public string PixelFormat { get; set; }

        /// <summary>
        /// 获取或设置重排序队列大小
        /// </summary>
        public int ReorderQueueSize { get; set; }

        /// <summary>
        /// 获取或设置是否启用低延迟标志
        /// </summary>
        public bool WithLowDelayFlags { get; set; }

        /// <summary>
        /// 获取或设置输出格式
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// 获取或设置输出质量配置
        /// </summary>
        public OutputQuality OutputQuality { get; set; }
    }
    /// <summary>
    /// 输出质量设置类
    /// </summary>
    public class OutputQuality
    {
        /// <summary>
        /// 获取或设置高质量设置
        /// </summary>
        public QualitySettings High { get; set; }

        /// <summary>
        /// 获取或设置中等质量设置
        /// </summary>
        public QualitySettings Medium { get; set; }

        /// <summary>
        /// 获取或设置低质量设置
        /// </summary>
        public QualitySettings Low { get; set; }
    }

    /// <summary>
    /// 质量设置类
    /// </summary>
    public class QualitySettings
    {
        /// <summary>
        /// 获取或设置比特率
        /// </summary>
        public int Bitrate { get; set; }

        /// <summary>
        /// 获取或设置速度预设
        /// </summary>
        public int SpeedPreset { get; set; }

        /// <summary>
        /// 获取或设置帧率模式
        /// </summary>
        public string FpsMode { get; set; }

        /// <summary>
        /// 获取或设置是否为零延迟
        /// </summary>
        public bool ZeroLatency { get; set; }

        /// <summary>
        /// 获取或设置关键帧间隔
        /// </summary>
        public int IntraFrame { get; set; }

        /// <summary>
        /// 获取或设置恒定码率因子
        /// </summary>
        public int ConstantRateFactor { get; set; }
    }
}
