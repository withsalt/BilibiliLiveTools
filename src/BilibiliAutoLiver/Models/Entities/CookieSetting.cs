using FreeSql;
using FreeSql.DataAnnotations;

namespace BilibiliAutoLiver.Models.Entities
{
    public class CookieSetting : IBaseEntity
    {
        /// <summary>
        /// Cookie内容
        /// </summary>
        [Column(DbType = "text", IsNullable = true)]
        public string Content { get; set; }
    }
}
