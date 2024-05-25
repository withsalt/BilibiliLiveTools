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
        public bool IsAutoRetry { get; set; }

        /// <summary>
        /// 重试间隔时间
        /// </summary>
        public int RetryInterval { get; set; }

    }
}
