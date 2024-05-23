using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models.Base;
using BilibiliAutoLiver.Config;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Models.ViewModels;
using BilibiliAutoLiver.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Controllers
{
    [Authorize]
    public class MonitorController : Controller
    {
        private readonly ILogger<MonitorController> _logger;
        private readonly IMemoryCache _cache;
        private readonly IBilibiliAccountApiService _accountService;
        private readonly IBilibiliCookieService _cookieService;
        private readonly IBilibiliLiveApiService _liveApiService;
        private readonly IMonitorSettingRepository _repository;

        public MonitorController(ILogger<MonitorController> logger
            , IMemoryCache cache
            , IBilibiliAccountApiService accountService
            , IBilibiliCookieService cookieService
            , IBilibiliLiveApiService liveApiService
            , IMonitorSettingRepository repository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _cookieService = cookieService ?? throw new ArgumentNullException(nameof(cookieService));
            _liveApiService = liveApiService ?? throw new ArgumentNullException(nameof(liveApiService));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            MonitorSetting setting = await _repository.GetCacheAsync();
            MonitorIndexPageViewModel vm = new MonitorIndexPageViewModel()
            {
                MonitorSetting = setting
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<ResultModel<string>> UpdateRoomInfo([FromBody] MonitorRoomInfoUpdateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel<string>(-1, "参数错误");
                }
                MonitorSetting setting = await _repository.Where(p => !p.IsDeleted).OrderByDescending(p => p.CreatedTime).FirstAsync();
                if (setting == null)
                {
                    setting = new MonitorSetting();
                }
                if (request.IsEnabled)
                {
                    if (!setting.IsEnableEmailNotice
                        || string.IsNullOrEmpty(setting.MailAddress)
                        || string.IsNullOrEmpty(setting.MailAddress)
                        || string.IsNullOrEmpty(setting.Receivers))
                    {
                        return new ResultModel<string>(-1, "开启直播监控需要先配置邮箱设置");
                    }
                }

                setting.IsEnabled = request.IsEnabled;
                setting.RoomUrl = request.RoomUrl;
                setting.RoomId = request.RoomId;

                await _repository.InsertOrUpdateAsync(setting);
                _repository.RemoveCache();
                return new ResultModel<string>(0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新监控直播间信息失败，{ex.Message}");
                return new ResultModel<string>(-1, ex.Message);
            }
        }

        [HttpPost]
        public async Task<ResultModel<string>> UpdateEmailInfo([FromBody] MonitorEmailUpdateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel<string>(-1, "参数错误");
                }
                MonitorSetting setting = await _repository.Where(p => !p.IsDeleted).OrderByDescending(p => p.CreatedTime).FirstAsync();
                if (setting == null)
                {
                    setting = new MonitorSetting()
                    {
                        CreatedTime = DateTime.UtcNow,
                        CreatedUserId = GlobalConfigConstant.SYS_USERID
                    };
                }
                if (request.SmtpSsl && request.SmtpPort != 587 && request.SmtpPort != 465 && request.SmtpPort != 2525)
                {
                    return new ResultModel<string>(-1, "SMTP端口不正确");
                }
                if (!request.SmtpSsl && request.SmtpPort != 25)
                {
                    return new ResultModel<string>(-1, "SMTP端口不正确");
                }
                if (!IsValidEmail(request.MailAddress))
                {
                    return new ResultModel<string>(-1, "发件人地址不正确");
                }

                var receivers = request.Receivers
                    .Replace('；', ';')
                    .Replace('，', ';')
                    .Replace(',', ';')
                    .Trim().Split(';');
                foreach (var receiver in receivers)
                {
                    if (!IsValidEmail(receiver))
                    {
                        return new ResultModel<string>(-1, $"收件人地址{receiver}不正确");
                    }
                }

                setting.IsEnableEmailNotice = request.IsEnableEmailNotice;
                setting.SmtpServer = request.SmtpServer;
                setting.SmtpSsl = request.SmtpSsl;
                setting.SmtpPort = request.SmtpPort;
                setting.MailAddress = request.MailAddress;
                setting.MailName = request.MailName;
                setting.Password = request.Password;
                setting.Receivers = string.Join(';', receivers);
                setting.UpdatedTime = DateTime.UtcNow;
                setting.UpdatedUserId = GlobalConfigConstant.SYS_USERID;

                await _repository.InsertOrUpdateAsync(setting);
                _repository.RemoveCache();
                return new ResultModel<string>(0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新监控直播间信息失败，{ex.Message}");
                return new ResultModel<string>(-1, ex.Message);
            }
        }

        public bool IsValidEmail(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return false;
                }
                if (MailAddress.TryCreate(email, out var address) && address != null)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
