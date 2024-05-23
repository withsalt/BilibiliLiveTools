using System.ComponentModel.DataAnnotations;

namespace BilibiliAutoLiver.Models.Dtos
{
    public class MonitorEmailUpdateRequest
    {
        /// <summary>
        /// 是否启用邮件通知
        /// </summary>
        [Required]
        public bool IsEnableEmailNotice { get; set; }

        /// <summary>
        /// SMTP服务地址
        /// </summary>
        [Required]
        public string SmtpServer { get; set; }

        /// <summary>
        /// 是否启用SSL
        /// </summary>
        [Required]
        public bool SmtpSsl { get; set; }

        /// <summary>
        /// SMTP端口
        /// </summary>
        [Required]
        [Range(25, 2525)]
        public int SmtpPort { get; set; }

        /// <summary>
        /// 发件人地址
        /// </summary>
        [Required]
        public string MailAddress { get; set; }

        /// <summary>
        /// 发件人名称
        /// </summary>
        [Required]
        public string MailName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// 接收人
        /// </summary>
        [Required]
        public string Receivers { get; set; }
    }
}
