using FreeSql.DataAnnotations;

namespace BilibiliAutoLiver.Models.Entities
{
    public class MonitorSetting : IBaseEntity
    {
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 监控直播间名称
        /// </summary>
        public long RoomId { get; set; }

        /// <summary>
        /// 是否启用邮件通知
        /// </summary>
        public bool IsEnableEmailNotice { get; set; }

        /// <summary>
        /// SMTP服务地址
        /// </summary>
        [Column(StringLength = 255, IsNullable = false)]
        public string SmtpServer { get; set; }

        /// <summary>
        /// 是否启用SSL
        /// </summary>
        public bool SmtpSsl { get; set; } = false;

        /// <summary>
        /// SMTP端口
        /// </summary>
        public int SmtpPort { get; set; } = 25;

        /// <summary>
        /// 发件人地址
        /// </summary>
        [Column(StringLength = 255, IsNullable = false)]
        public string MailAddress { get; set; }

        /// <summary>
        /// 发件人名称
        /// </summary>
        [Column(StringLength = 255, IsNullable = false)]
        public string MailName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Column(StringLength = 255, IsNullable = false)]
        public string Password { get; set; }

        /// <summary>
        /// 接收人
        /// </summary>
        [Column(StringLength = 255, IsNullable = false)]
        public string Receivers { get; set; }
    }
}
