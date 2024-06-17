using System;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models.Base;
using BilibiliAutoLiver.Config;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Models.ViewModels;
using BilibiliAutoLiver.Repository.Interface;
using BilibiliAutoLiver.Services.Interface;
using BilibiliAutoLiver.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Controllers
{
    [Authorize]
    public class PushController : Controller
    {
        private readonly ILogger<PushController> _logger;
        private readonly IMemoryCache _cache;
        private readonly IBilibiliAccountApiService _accountService;
        private readonly IBilibiliCookieService _cookieService;
        private readonly IBilibiliLiveApiService _liveApiService;
        private readonly IPushSettingRepository _pushSettingRepository;
        private readonly IPushStreamProxyService _proxyService;

        public PushController(ILogger<PushController> logger
            , IMemoryCache cache
            , IBilibiliAccountApiService accountService
            , IBilibiliCookieService cookieService
            , IBilibiliLiveApiService liveApiService
            , ILiveSettingRepository liveSettingRepos
            , IPushSettingRepository pushSettingRepository
            , IPushStreamProxyService proxyService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _cookieService = cookieService ?? throw new ArgumentNullException(nameof(cookieService));
            _liveApiService = liveApiService ?? throw new ArgumentNullException(nameof(liveApiService));
            _pushSettingRepository = pushSettingRepository ?? throw new ArgumentNullException(nameof(pushSettingRepository));
            _proxyService = proxyService ?? throw new ArgumentNullException(nameof(proxyService));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            PushIndexPageViewModel vm = new PushIndexPageViewModel();
            vm.PushSetting = await _pushSettingRepository.Where(p => !p.IsDeleted).FirstAsync();
            if (vm.PushSetting == null)
            {
                throw new Exception("获取推流配置失败！");
            }
            return View(vm);
        }

        public async Task<ResultModel<string>> Update([FromBody] PushSettingUpdateRequest request)
        {
            PushSetting setting = await _pushSettingRepository.Where(p => !p.IsDeleted).FirstAsync();
            if (setting == null)
            {
                throw new Exception("获取推流配置失败！");
            }
            ResultModel<string> modelUpdateResult = null;
            switch (request.Model)
            {
                case ConfigModel.Normal:
                    modelUpdateResult = UpdateEasyModel(request, setting);
                    break;
                case ConfigModel.Advance:
                    modelUpdateResult = UpdateAdvanceModel(request, setting);
                    break;
                default:
                    throw new NotSupportedException("参数错误，未知的设置类型");
            }
            if (modelUpdateResult.Code != 0)
            {
                return modelUpdateResult;
            }
            //更新重试配置
            setting.IsAutoRetry = request.IsAutoRetry;
            setting.RetryInterval = request.RetryInterval;
            setting.UpdatedTime = DateTime.UtcNow;
            setting.UpdatedUserId = GlobalConfigConstant.SYS_USERID;
            setting.IsUpdate = true;

            int updateResult = await _pushSettingRepository.UpdateAsync(setting);
            if (updateResult <= 0)
            {
                throw new Exception("保存配置信息失败！");
            }
            return new ResultModel<string>(0);
        }

        private ResultModel<string> UpdateAdvanceModel(PushSettingUpdateRequest request, PushSetting setting)
        {
            if (setting.Model == ConfigModel.Normal)
            {
                setting.Model = ConfigModel.Advance;
            }
            if (string.IsNullOrWhiteSpace(setting.FFmpegCommand))
            {
                return new ResultModel<string>(-1, "推流命令不能为空");
            }
            //解析推流命令 
            if (!CmdAnalyzer.TryParse(request.FFmpegCommand, true, out string message, out _))
            {
                return new ResultModel<string>(-1, message);
            }
            setting.FFmpegCommand = request.FFmpegCommand;
            return new ResultModel<string>(0);
        }

        private ResultModel<string> UpdateEasyModel(PushSettingUpdateRequest request, PushSetting setting)
        {
            if (setting.Model == ConfigModel.Advance)
            {
                setting.Model = ConfigModel.Normal;
            }
            return new ResultModel<string>(0);
        }

        /// <summary>
        /// 获取当前推流状态
        /// </summary>
        /// <returns></returns>
        public ResultModel<PushStatusResponse> Status()
        {
            var status = _proxyService.GetStatus();

            return new ResultModel<PushStatusResponse>(0)
            {
                Data = new PushStatusResponse()
                {
                    Status = status,
                }
            };
        }
    }
}
