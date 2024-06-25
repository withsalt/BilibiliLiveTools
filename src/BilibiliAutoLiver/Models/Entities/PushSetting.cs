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

        #region 高级模式

        /// <summary>
        /// 配置内容
        /// </summary>
        [Column(DbType = "text")]
        public string FFmpegCommand { get; set; } = "{}";

        #endregion

        #region 简易模式

        /// <summary>
        /// 输入类型
        /// </summary>
        public InputType InputType { get; set; }

        /// <summary>
        /// 输出分辨率
        /// </summary>
        [MaxLength(64)]
        [Column(IsNullable = false)]
        public string OutputResolution { get; set; }

        /// <summary>
        /// 自定义输出参数
        /// </summary>
        [MaxLength(512)]
        [Column(IsNullable = true)]
        public string CustumOutputParams { get; set; }

        /// <summary>
        /// 推流视频Id
        /// </summary>
        public long VideoId {  get; set; }

        /// <summary>
        /// 是否静音
        /// </summary>
        public bool IsMute {  get; set; }

        /// <summary>
        /// 推流音频Id
        /// </summary>
        public long? AudioId {  get; set; }

        #endregion

        #region 自动重试

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

        #endregion

    }
}
