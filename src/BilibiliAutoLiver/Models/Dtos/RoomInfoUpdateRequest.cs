using System.ComponentModel.DataAnnotations;

namespace BilibiliAutoLiver.Models.Dtos
{
    public class RoomInfoUpdateRequest
    {
        /// <summary>
        /// 直播间分区Id
        /// </summary>
        [Required]
        [Range(1, 10000)]
        public int AreaId { get; set; }

        /// <summary>
        /// 直播间名称
        /// </summary>
        [Required]
        public string RoomName { get; set; }

        /// <summary>
        /// 直播间Id
        /// </summary>
        [Required]
        public long RoomId { get; set; }
    }
}
