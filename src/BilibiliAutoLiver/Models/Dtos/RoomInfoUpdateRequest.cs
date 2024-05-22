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

        /// <summary>
        /// 是否开启自动重试
        /// </summary>
        [Required]
        [Range(0, 1)]
        public int IsAutoRetryValue { get; set; }

        public bool IsAutoRetry
        {
            get
            {
                return IsAutoRetryValue == 1;
            }
        }

        /// <summary>
        /// 重试间隔时间
        /// </summary>
        [Required]
        [Range(30, 10000)]
        public int RetryInterval { get; set; }
    }
}
