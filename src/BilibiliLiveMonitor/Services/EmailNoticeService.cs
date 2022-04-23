using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using BilibiliLiveMonitor.Configs;

namespace BilibiliLiveMonitor.Services
{
    public class EmailNoticeService : IEmailNoticeService
    {
        private readonly AppSettingsNode _appSettings;

        /// <summary>
        /// 发送标题和内容的字符集，默认是UTF8
        /// </summary>
        public Encoding CharSet = Encoding.UTF8;

        /// <summary>
        /// 是否使用抄送
        /// </summary>
        public bool IsCC = false;

        /// <summary>
        /// 是否开启安全套接字层 (SSL) 加密连接
        /// </summary>
        public bool IsSSL = false;

        /// <summary>
        /// SMTP服务器是否不需要身份认证
        /// 默认为False，需要使用账号和密码登陆
        /// </summary>
        public bool IsUseDefaultCredentials = false;

        /// <summary>
        /// 邮箱正则表达式(不区分大小写)，可修改成自定义正则表达式
        /// </summary>
        public string RegexText = @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";

        /// <summary>
        /// Smtp 服务器端口,默认25
        /// </summary>
        public int SmtpPort = 25;

        /// <summary>
        /// 邮件的抄送者
        /// 如果确定发送邮件的抄送者请将IsCC设置为true
        /// </summary>
        public string[] CarbonCopy { get; set; }

        /// <summary>
        /// 收件邮箱地址
        /// </summary>
        public string[] ReceiveMailAddress { get; set; }

        /// <summary>
        /// 邮箱密码
        /// </summary>
        public string PassWord { get; set; }

        /// <summary>
        ///  发件邮箱地址
        /// </summary>
        public string SendMailAddress { get; set; }

        /// <summary>
        ///  发件邮箱昵称 ,默认为发件地址
        /// </summary>
        public string SendMailName { get; set; }

        /// <summary>
        /// Smtp 服务器地址
        /// </summary>
        public string SmtpServer { get; set; }

        public EmailNoticeService(IOptions<AppSettingsNode> appSettingsOptions)
        {
            _appSettings = appSettingsOptions.Value ?? throw new ArgumentNullException(nameof(appSettingsOptions));
            //开启了邮件发送才初始化
            if (_appSettings.IsEnableEmailNotice)
            {
                this.SmtpServer = _appSettings.EmailConfig.SmtpServer;
                this.SmtpPort = (int)_appSettings.EmailConfig.SmtpPort; 
                this.SendMailAddress = _appSettings.EmailConfig.SendMailAddress; 
                this.PassWord = _appSettings.EmailConfig.Password; 
                this.ReceiveMailAddress = _appSettings.EmailConfig.ReceiveMailAddress.Split(',');
                this.IsSSL = _appSettings.EmailConfig.SmtpSsl; 
                this.SendMailName = _appSettings.EmailConfig.SendMailName;
            }
        }

        /// <summary>
        /// 发送邮件，并且返回发送结果
        /// </summary>
        /// <returns></returns>
        public async Task<(SendStatus, string)> Send(string title
            , string body
            , string[] attachments = null
            , bool isBodyHtml = false)
        {
            if (!_appSettings.IsEnableEmailNotice)
            {
                return (SendStatus.Failed, "请先打开配置文件中邮件发送开关。");
            }

            #region 验证基本信息

            Regex reg = new Regex(this.RegexText, RegexOptions.IgnoreCase);
            if (!IsUseDefaultCredentials) // 使用账号密码登陆Smtp服务
            {
                if (!reg.IsMatch(this.SendMailAddress))
                {
                    return (SendStatus.SendMailAddressIsUnmatch, "发件地址未通过验证");
                }
                if (string.IsNullOrEmpty(this.PassWord))
                {
                    return (SendStatus.PassWordIsNull, "发件密码为空");
                }
            }

            if (this.ReceiveMailAddress == null || this.ReceiveMailAddress.Length <= 0)
            {
                return (SendStatus.FromMailAddressIsUnmatch, "收件地址为空");
            }

            StringBuilder FailedELst = new StringBuilder();
            foreach (string item in this.ReceiveMailAddress)
            {
                if (!reg.IsMatch(item))
                {
                    FailedELst.Append(Environment.NewLine + item);
                }
            }
            if (FailedELst.Length > 0)
            {
                return (SendStatus.FromMailAddressIsUnmatch, "以下收件地址未通过验证:" + FailedELst);
            }

            #endregion 验证基本信息

            SmtpClient smtp = new SmtpClient(); //实例化一个Smtp
            smtp.EnableSsl = this.IsSSL; //smtp服务器是否启用SSL加密
            smtp.Host = this.SmtpServer; //指定 Smtp 服务器地址
            smtp.Port = this.SmtpPort;  //指定 Smtp 服务器的端口，默认是465 ,587
            if (IsUseDefaultCredentials)   // SMTP服务器是否不需要身份认证
                smtp.UseDefaultCredentials = true;
            else
                smtp.Credentials = new NetworkCredential(this.SendMailAddress, this.PassWord); // NetworkCredential("邮箱名", "密码");

            MailMessage mm = new MailMessage
            {
                //邮件的优先级
                Priority = MailPriority.Normal,
                //收件方看到的邮件来源；第一个参数是发信人邮件地址第二参数是发信人显示的名称第三个参数是, Encoding.GetEncoding(936) 第二个参数所使用的编码，如果指定不正确，则对方收到后显示乱码，936是简体中文的codepage值
                From = new MailAddress(this.SendMailAddress, this.SendMailName, Encoding.UTF8)
            };
            //ReplyTo 表示对方回复邮件时默认的接收地址，即：你用一个邮箱发信，但却用另一个来收信后两个参数的意义， 同 From 的意义
            mm.ReplyToList.Add(new MailAddress(this.SendMailAddress, this.SendMailName, Encoding.UTF8));

            StringBuilder fmLst = new StringBuilder();
            foreach (string item in this.ReceiveMailAddress)
            {
                fmLst.Append(item + ",");
            }
            mm.To.Add(fmLst.Remove(fmLst.Length - 1, 1).ToString()); //邮件的接收者，支持群发，多个地址之间用 半角逗号 分开
            if (this.CarbonCopy != null && this.CarbonCopy.Length > 0 && this.IsCC) // 是否显示抄送
            {
                StringBuilder ccLst = new StringBuilder();
                foreach (string item in this.CarbonCopy)
                {
                    ccLst.Append(item + ",");
                }
                mm.CC.Add(ccLst.Remove(ccLst.Length - 1, 1).ToString()); //邮件的抄送者，支持群发，多个邮件地址之间用 半角逗号 分开
            }
            mm.Subject = title; //邮件标题
            mm.SubjectEncoding = CharSet; // 这里非常重要，如果你的邮件标题包含中文，这里一定要指定，否则对方收到的极有可能是乱码。// 936是简体中文的pagecode，如果是英文标题，这句可以忽略不用
            mm.IsBodyHtml = isBodyHtml; //邮件正文是否是HTML格式
            mm.BodyEncoding = CharSet; //邮件正文的编码， 设置不正确， 接收者会收到乱码
            mm.Body = body;//邮件正文

            if (attachments != null && attachments.Length > 0)
            {
                foreach (string item in attachments)
                {
                    mm.Attachments.Add(new Attachment(item));  //添加附件
                }
            }

            try
            {
                await smtp.SendMailAsync(mm);
                return (SendStatus.Success, "邮件发送成功");
            }
            catch (Exception ex)
            {
                return (SendStatus.Failed, "发送失败，" + ex.Message);
            }
        }
    }

    /// <summary>
    /// 邮件发送状态
    /// </summary>
    public enum SendStatus
    {
        /// <summary>
        /// 默认,正常,准备发送
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 发送成功
        /// </summary>
        Success = 1,

        /// <summary>
        /// 发送失败
        /// </summary>
        Failed = 2,

        /// <summary>
        /// 发送邮箱或收件邮箱未通过验证
        /// </summary>
        Unmatch = 3,

        /// <summary>
        /// 发件地址未通过验证
        /// </summary>
        SendMailAddressIsUnmatch = 4,

        /// <summary>
        /// 发件密码为空
        /// </summary>
        PassWordIsNull = 5,

        /// <summary>
        /// 收件地址未通过验证
        /// </summary>
        FromMailAddressIsUnmatch = 6
    }
}
