using System.ComponentModel.DataAnnotations;

namespace BilibiliAutoLiver.Models.Dtos
{
    public class RoomNewUpdateRequest
    {
        /// <summary>
        /// 直播间Id
        /// </summary>
        [Required]
        public long RoomId { get; set; }

        /// <summary>
        /// 直播间公告
        /// </summary>
        [MaxLength(60, ErrorMessage = "公告内容不能超过60个字符")]
        public string Content { get; set; }
    }
}
