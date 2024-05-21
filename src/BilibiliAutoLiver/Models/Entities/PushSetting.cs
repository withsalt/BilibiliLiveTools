using System.ComponentModel.DataAnnotations;
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
        /// 配置内容
        /// </summary>
        [Column(DbType = "text")]
        public string Content { get; set; } = "{}";

        /// <summary>
        /// 描述
        /// </summary>
        [Column(StringLength = 600, IsNullable = true)]
        public string Description { get; set; }
    }
}
