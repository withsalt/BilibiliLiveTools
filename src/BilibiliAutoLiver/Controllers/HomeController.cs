using System;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models;
using BilibiliAutoLiver.Extensions;
using BilibiliAutoLiver.Models;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Models.ViewModels;
using BilibiliAutoLiver.Repository.Interface;
using BilibiliAutoLiver.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMemoryCache _cache;
        private readonly IBilibiliAccountApiService _accountService;
        private readonly IBilibiliCookieService _cookieService;
        private readonly IBilibiliLiveApiService _liveApiService;
        private readonly IPushStreamProxyService _proxyService;
        private readonly IFFMpegService _ffmpeg;
        private readonly ILiveSettingRepository _liveSettingRepository;

        public HomeController(ILogger<HomeController> logger
            , IMemoryCache cache
            , IBilibiliAccountApiService accountService
            , IBilibiliCookieService cookieService
            , IBilibiliLiveApiService liveApiService
            , IPushStreamProxyService proxyService
            , IFFMpegService ffmpeg
            , ILiveSettingRepository liveSettingRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _cookieService = cookieService ?? throw new ArgumentNullException(nameof(cookieService));
            _liveApiService = liveApiService ?? throw new ArgumentNullException(nameof(liveApiService));
            _proxyService = proxyService ?? throw new ArgumentNullException(nameof(proxyService));
            _ffmpeg = ffmpeg ?? throw new ArgumentNullException(nameof(ffmpeg));
            _liveSettingRepository = liveSettingRepository ?? throw new ArgumentNullException(nameof(liveSettingRepository));
        }

        public async Task<IActionResult> Index()
        {
            var userInfo = await _accountService.GetUserInfo();
            if (userInfo == null)
            {
                return RedirectToAction("Logout", "Account");
            }
            return View(new HomeIndexPageViewModel()
            {
                UserInfo = userInfo,
            });
        }

        public async Task<IActionResult> Console()
        {
            if (_proxyService.GetStatus() != PushStatus.Stopped)
            {
                return View(await BuildStartedViewModel());
            }
            else
            {
                return View(await BuildStoppedViewModel());
            }
        }

        private async Task<ConsolePageViewModel> BuildStoppedViewModel()
        {
            string version = await TryGetFFMPEGVersion();
            LiveSetting roomInfo = await GetRoomInfo();

            ConsolePageViewModel vm = new ConsolePageViewModel()
            {
                Status = EnumExtensions.GetEnumDescription(_proxyService.GetStatus()),
                Version = version,
                RoomName = roomInfo.RoomName,
                Time = "未开启"
            };
            return vm;
        }

        private async Task<ConsolePageViewModel> BuildStartedViewModel()
        {
            string version = await TryGetFFMPEGVersion();
            LiveSetting roomInfo = await GetRoomInfo();
            LiveRoomInfo infoRt = await _liveApiService.GetLiveRoomInfo(roomInfo.RoomId);
            string time = "未知";
            if (infoRt.is_living && DateTime.TryParse(infoRt.live_time, out DateTime liveTime))
            {
                time = FormatMinutes((int)((DateTime.Now - liveTime).TotalMinutes));
            }
            else if (infoRt.is_living)
            {
                time = "未开启";
            }

            ConsolePageViewModel vm = new ConsolePageViewModel()
            {
                Status = EnumExtensions.GetEnumDescription(_proxyService.GetStatus()),
                Version = version,
                RoomName = roomInfo.RoomName,
                Time= time,
            };
            return vm;
        }

        private async Task<string> TryGetFFMPEGVersion()
        {
            string version = null;
            try
            {
                version = (await _ffmpeg.GetVersion()).Version;
                if (version.Length > 10)
                {
                    version = version.Substring(0, 10) + "...";
                }
            }
            catch
            {
                version = "";
            }
            return version;
        }

        private async Task<LiveSetting> GetRoomInfo()
        {
            LiveSetting setting = await _liveSettingRepository.Where(p => !p.IsDeleted).FirstAsync();
            if (setting == null)
            {
                var roomInfo = await _liveApiService.GetMyLiveRoomInfo();
                return new LiveSetting()
                {
                    RoomName = roomInfo.title,
                    RoomId = roomInfo.room_id,
                };
            }
            return setting;
        }

        private string FormatMinutes(int totalMinutes)
        {
            int days = totalMinutes / (24 * 60);
            int hours = (totalMinutes % (24 * 60)) / 60;
            int minutes = totalMinutes % 60;

            string result = "";
            if (days > 0)
                result += $"{days}天";
            if (hours > 0)
                result += $"{hours}小时";
            if (minutes > 0)
                result += $"{minutes}分钟";

            return string.IsNullOrEmpty(result) ? "0分钟" : result;
        }
    }
}
