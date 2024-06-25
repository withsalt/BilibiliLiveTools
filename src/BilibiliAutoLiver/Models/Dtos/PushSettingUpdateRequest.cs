using BilibiliAutoLiver.Models.Enums;

namespace BilibiliAutoLiver.Models.Dtos
{
    public class PushSettingUpdateRequest
    {
        public ConfigModel Model { get; set; }

        /// <summary>
        /// 配置内容
        /// </summary>
        public string FFmpegCommand { get; set; }

        /// <summary>
        /// 是否开启自动重试
        /// </summary>
        public bool IsAutoRetry { get; set; }

        /// <summary>
        /// 重试间隔时间
        /// </summary>
        public int RetryInterval { get; set; }

        #region 简易模式

        /// <summary>
        /// 输入类型
        /// </summary>
        public InputType InputType { get; set; }

        /// <summary>
        /// 输出分辨率
        /// </summary>
        public string OutputResolution { get; set; }

        /// <summary>
        /// 自定义输出参数
        /// </summary>
        public string CustumOutputParams { get; set; }

        /// <summary>
        /// 推流视频Id
        /// </summary>
        public long VideoId { get; set; }

        /// <summary>
        /// 是否静音
        /// </summary>
        public bool IsMute { get; set; }

        /// <summary>
        /// 推流音频Id
        /// </summary>
        public long? AudioId { get; set; }

        #endregion

    }
}
