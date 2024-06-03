using System.ComponentModel.DataAnnotations;

namespace BilibiliAutoLiver.Models.Dtos
{
    public class RoomInfoUpdateRequest
    {
        /// <summary>
        /// 直播间分区Id
        /// </summary>
        [Required]
        [Range(1, 1000000)]
        public int AreaId { get; set; }

        /// <summary>
        /// 直播间名称
        /// </summary>
        [Required(ErrorMessage = "直播间名称不能为空")]
        [MaxLength(12, ErrorMessage = "直播间名称不能超过12个字符")]
        public string RoomName { get; set; }

        /// <summary>
        /// 直播间Id
        /// </summary>
        [Required]
        public long RoomId { get; set; }
    }
}
