using FreeSql.DataAnnotations;

namespace BilibiliAutoLiver.Models.Entities
{
    public class LiveSetting : IBaseEntity
    {
        /// <summary>
        /// 直播间分区Id
        /// </summary>
        public int AreaId { get; set; }

        /// <summary>
        /// 直播间名称
        /// </summary>
        [Column(StringLength = 300, IsNullable = false)]
        public string RoomName { get; set; }

        /// <summary>
        /// 直播间Id
        /// </summary>
        public long RoomId { get; set; }

        /// <summary>
        /// 直播间公告
        /// </summary>
        [Column(StringLength = 300, IsNullable = true)]
        public string Content { get; set; }
    }
}
