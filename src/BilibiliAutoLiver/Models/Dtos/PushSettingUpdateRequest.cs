using System.ComponentModel.DataAnnotations;
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
        [Required(ErrorMessage = "参数是否开启不间断直播不能为空")]
        public bool IsAutoRetry { get; set; }

        /// <summary>
        /// 重试间隔时间
        /// </summary>
        [Required(ErrorMessage = "参数重试间隔时间不能为空")]
        [Range(30, int.MaxValue, ErrorMessage = "重试间隔时间不能小于30秒")]
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

        /// <summary>
        /// 推流桌面区域
        /// </summary>
        public string InputScreen { get; set; }

        /// <summary>
        /// 推流桌面时，音频id
        /// </summary>
        public long? DesktopAudioId { get; set; }

        /// <summary>
        /// 推流桌面时，来源音频设备
        /// </summary>
        public string DesktopAudioDeviceId{ get; set; }

        /// <summary>
        /// 推流音频来源
        /// </summary>
        public bool DesktopAudioFrom {  get; set; }

        #endregion

    }
}
