using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Repository.Interface;
using BilibiliAutoLiver.Services.Interface;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;

namespace BilibiliAutoLiver.Services
{
    public class EmailNoticeService : IEmailNoticeService
    {
        private readonly IMonitorSettingRepository _settingRepository;
        private readonly ILogger<EmailNoticeService> _logger;


        public EmailNoticeService(IMonitorSettingRepository settingRepository
            , ILogger<EmailNoticeService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settingRepository = settingRepository ?? throw new ArgumentNullException(nameof(settingRepository));
        }

        /// <summary>
        /// 发送邮件，并且返回发送结果
        /// </summary>
        /// <returns></returns>
        public async Task<(SendStatus, string)> Send(string title, string body)
        {
            try
            {
                MonitorSetting setting = await _settingRepository.GetCacheAsync();
                if (setting == null || !setting.IsEnabled)
                {
                    return (SendStatus.Failed, "获取发送配置失败，或未启用直播监控");
                }
                if (!setting.IsEnableEmailNotice)
                {
                    return (SendStatus.Disabled, "请先打开配置文件中邮件发送开关。");
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(setting.MailName, setting.MailAddress));
                message.ReplyTo.Add(new MailboxAddress(setting.MailName, setting.MailAddress));
                message.Subject = title;
                message.Body = new TextPart(TextFormat.Text)
                {
                    Text = body,
                };

                var receiveMailAddress = setting.Receivers?.Split(";");
                if (receiveMailAddress == null || receiveMailAddress.Length <= 0)
                {
                    return (SendStatus.FromMailAddressIsUnmatch, "收件地址为空");
                }
                List<string> failedELst = new List<string>();
                foreach (var email in receiveMailAddress)
                {
                    if (MailAddress.TryCreate(email, out var mailAddress) && mailAddress != null)
                        message.To.Add(new MailboxAddress(Encoding.UTF8, mailAddress.User, mailAddress.Address));
                    else
                        failedELst.Add(email);
                }
                if (failedELst.Count > 0)
                {
                    return (SendStatus.FromMailAddressIsUnmatch, "以下收件地址未通过验证:" + string.Join(',', failedELst));
                }

                if (!MailAddress.TryCreate(setting.MailAddress, out var address))
                {
                    return (SendStatus.SendMailAddressIsUnmatch, "发件地址未通过验证");
                }
                if (string.IsNullOrEmpty(setting.Password))
                {
                    return (SendStatus.PassWordIsNull, "发件密码为空");
                }

                MailKit.Net.Smtp.SmtpClient client = new MailKit.Net.Smtp.SmtpClient();
                try
                {
                    await client.ConnectAsync(setting.SmtpServer, setting.SmtpPort, setting.SmtpSsl);
                    await client.AuthenticateAsync(setting.MailAddress, setting.Password);
                    var rt = await client.SendAsync(message);
                    _logger.LogInformation($"邮件发送完成：{rt}");
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (client != null)
                    {
                        await client.DisconnectAsync(true);
                        client.Dispose();
                    }
                }

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
        FromMailAddressIsUnmatch = 6,

        /// <summary>
        /// 禁用邮件发送
        /// </summary>
        Disabled = 7,
    }
}
