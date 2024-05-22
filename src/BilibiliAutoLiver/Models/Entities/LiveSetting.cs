using FreeSql.DataAnnotations;

namespace BilibiliAutoLiver.Models.Entities
{
    public class LiveSetting : IBaseEntity
    {
        /// <summary>
        /// 直播间分区Id
        /// </summary>
        public int AreaId {  get; set; }

        /// <summary>
        /// 直播间名称
        /// </summary>
        [Column(StringLength = 300, IsNullable = false)]
        public string RoomName {  get; set; }

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
