using System.ComponentModel.DataAnnotations;

namespace BilibiliAutoLiver.Models.Dtos
{
    public class MonitorRoomInfoUpdateRequest
    {
        [Required]
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 监控直播间Id
        /// </summary>
        [Required]
        [Range(100000, long.MaxValue)]
        public long RoomId { get; set; }

        /// <summary>
        /// 直播间地址
        /// </summary>
        [Required]
        public string RoomUrl { get; set; }
    }
}
