using System.ComponentModel.DataAnnotations;
using BilibiliAutoLiver.Models.Enums;
using FreeSql.DataAnnotations;

namespace BilibiliAutoLiver.Models.Entities
{
    /// <summary>
    /// 配置项
    /// </summary>
    public class PushSetting : IBaseEntity
    {
        /// <summary>
        /// 键名
        /// </summary>
        [MaxLength(120)]
        [Column(IsNullable = false)]
        public string Key { get; set; }

        /// <summary>
        /// 配置模式
        /// </summary>
        public ConfigModel Model { get; set; }

        /// <summary>
        /// 配置内容
        /// </summary>
        [Column(DbType = "text")]
        public string FFmpegCommand { get; set; } = "{}";

        /// <summary>
        /// 是否开启自动重试
        /// </summary>
        public bool IsAutoRetry { get; set; }

        /// <summary>
        /// 重试间隔时间
        /// </summary>
        public int RetryInterval { get; set; }

        /// <summary>
        /// 是否更新了推流设置
        /// </summary>
        public bool IsUpdate { get; set; }
    }
}
