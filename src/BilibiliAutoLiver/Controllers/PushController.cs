using System;
using System.IO;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models.Base;
using BilibiliAutoLiver.Config;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Models.Settings;
using BilibiliAutoLiver.Models.ViewModels;
using BilibiliAutoLiver.Repository.Interface;
using BilibiliAutoLiver.Services.Interface;
using BilibiliAutoLiver.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        private readonly AppSettings _appSettings;
        private readonly IFFMpegService _ffmpeg;

        public PushController(ILogger<PushController> logger
            , IMemoryCache cache
            , IBilibiliAccountApiService accountService
            , IBilibiliCookieService cookieService
            , IBilibiliLiveApiService liveApiService
            , ILiveSettingRepository liveSettingRepos
            , IPushSettingRepository pushSettingRepository
            , IPushStreamProxyService proxyService
            , IOptions<AppSettings> settingOptions
            , IFFMpegService ffmpeg)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _cookieService = cookieService ?? throw new ArgumentNullException(nameof(cookieService));
            _liveApiService = liveApiService ?? throw new ArgumentNullException(nameof(liveApiService));
            _pushSettingRepository = pushSettingRepository ?? throw new ArgumentNullException(nameof(pushSettingRepository));
            _proxyService = proxyService ?? throw new ArgumentNullException(nameof(proxyService));
            _appSettings = settingOptions?.Value ?? throw new ArgumentNullException(nameof(settingOptions));
            _ffmpeg = ffmpeg ?? throw new ArgumentNullException(nameof(ffmpeg));
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
            vm.ListDeviceFFmpegCmd = _ffmpeg.GetBinaryPath() + " -list_devices true -f dshow -i dummy";
            return View(vm);
        }

        [HttpPost]
        public async Task<ResultModel<string>> Update([FromBody] PushSettingUpdateRequest request)
        {
            PushSetting setting = await _pushSettingRepository.Where(p => !p.IsDeleted).FirstAsync();
            if (setting == null)
            {
                return new ResultModel<string>(-1, "获取推流配置失败");
            }
            if (request.Model == ConfigModel.Normal)
            {
                return new ResultModel<string>(-1, "暂不支持简易模式，正在开发，不好写啊！");
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
                    return new ResultModel<string>(-1, "参数错误，未知的设置类型");
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
                return new ResultModel<string>(-1, "保存配置信息失败");
            }
            if (_proxyService.GetStatus() != PushStatus.Stopped)
            {
                return new ResultModel<string>(1);
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
            if (!CmdAnalyzer.TryParse(request.FFmpegCommand, _appSettings.AdvanceStrictMode, Path.Combine(_appSettings.DataDirectory, GlobalConfigConstant.DefaultMediaDirectory), "", out string message, out _, out _, out _))
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

        [HttpPost]
        public async Task<ResultModel<string>> Stop()
        {
            try
            {
                if (_proxyService.GetStatus() == PushStatus.Stopped)
                {
                    return new ResultModel<string>(0);
                }
                bool result = await _proxyService.Stop();
                if (!result)
                {
                    return new ResultModel<string>(-1, "停止推流失败");
                }
                else
                {
                    return new ResultModel<string>(0);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止推流失败");
                return new ResultModel<string>(-1, $"停止推流失败，{ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ResultModel<string>> Start()
        {
            try
            {
                if (_proxyService.GetStatus() != PushStatus.Stopped)
                {
                    return new ResultModel<string>(0);
                }
                bool result = await _proxyService.Start();
                if (!result)
                {
                    return new ResultModel<string>(-1, "开启推流失败");
                }
                else
                {
                    return new ResultModel<string>(0);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "开启推流失败");
                return new ResultModel<string>(-1, $"开启推流失败，{ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ResultModel<string>> ReStart()
        {
            try
            {
                if (_proxyService.GetStatus() != PushStatus.Stopped)
                {
                    bool stoprResult = await _proxyService.Stop();
                    if (!stoprResult)
                    {
                        return new ResultModel<string>(-1, "重新推流失败，停止当前正在进行中的推流失败。");
                    }
                }
                bool result = await _proxyService.Start();
                if (!result)
                {
                    return new ResultModel<string>(-1, "重新推流失败，开启推流失败");
                }
                else
                {
                    return new ResultModel<string>(0);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重新推流失败");
                return new ResultModel<string>(-1, $"重新推流失败，{ex.Message}");
            }
        }

        /// <summary>
        /// 获取当前推流状态
        /// </summary>
        /// <returns></returns>
        [HttpGet]
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
