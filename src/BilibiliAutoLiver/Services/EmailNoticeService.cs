using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Repository.Interface;
using BilibiliAutoLiver.Services.Interface;

namespace BilibiliAutoLiver.Services
{
    public class EmailNoticeService : IEmailNoticeService
    {
        /// <summary>
        /// 邮箱正则表达式(不区分大小写)，可修改成自定义正则表达式
        /// </summary>
        private string RegexText = @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";

        private readonly IMonitorSettingRepository _settingRepository;

        public EmailNoticeService(IMonitorSettingRepository settingRepository)
        {
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

                #region 验证基本信息

                Regex reg = new Regex(RegexText, RegexOptions.IgnoreCase);
                if (!reg.IsMatch(setting.MailAddress))
                {
                    return (SendStatus.SendMailAddressIsUnmatch, "发件地址未通过验证");
                }
                if (string.IsNullOrEmpty(setting.Password))
                {
                    return (SendStatus.PassWordIsNull, "发件密码为空");
                }

                var receiveMailAddress = setting.Receivers?.Split(";");
                if (receiveMailAddress == null || receiveMailAddress.Length <= 0)
                {
                    return (SendStatus.FromMailAddressIsUnmatch, "收件地址为空");
                }

                StringBuilder FailedELst = new StringBuilder();
                foreach (string item in receiveMailAddress)
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

                MailMessage mm = new MailMessage
                {
                    Priority = MailPriority.Normal,
                    From = new MailAddress(setting.MailAddress, setting.MailName, Encoding.UTF8)
                };
                //ReplyTo 表示对方回复邮件时默认的接收地址，即：你用一个邮箱发信，但却用另一个来收信后两个参数的意义， 同 From 的意义
                mm.ReplyToList.Add(new MailAddress(setting.MailAddress, setting.MailName, Encoding.UTF8));
                mm.To.Add(string.Join(',', receiveMailAddress.Where(p => !string.IsNullOrWhiteSpace(p))));
                //抄送
                //if (carbonCopy != null && carbonCopy.Length > 0 && isCC)
                //{
                //    mm.CC.Add(string.Join(',', carbonCopy.Where(p => !string.IsNullOrWhiteSpace(p))));
                //}
                //附件
                //if (attachments != null && attachments.Length > 0)
                //{
                //    foreach (string item in attachments)
                //    {
                //        mm.Attachments.Add(new Attachment(item));  //添加附件
                //    }
                //}
                mm.Subject = title;
                mm.SubjectEncoding = Encoding.UTF8;
                mm.IsBodyHtml = false;
                mm.BodyEncoding = Encoding.UTF8;
                mm.Body = body;

                SmtpClient smtp = new SmtpClient
                {
                    EnableSsl = setting.SmtpSsl,
                    Host = setting.SmtpServer,
                    Port = setting.SmtpPort,
                    Credentials = new NetworkCredential(setting.MailAddress, setting.Password)
                };
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
        FromMailAddressIsUnmatch = 6,

        /// <summary>
        /// 禁用邮件发送
        /// </summary>
        Disabled = 7,
    }
}
