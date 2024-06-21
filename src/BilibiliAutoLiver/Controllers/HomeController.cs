using System;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using BilibiliAutoLiver.Extensions;
using BilibiliAutoLiver.Models;
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
                version = "δ֪";
            }

            string name = "";
            var setting = await _liveSettingRepository.Where(p => !p.IsDeleted).FirstAsync();
            if (setting == null)
            {
                var roomInfo = await _liveApiService.GetMyLiveRoomInfo();
                name = roomInfo.title;
            }
            else
            {
                name = setting.RoomName;
            }

            ConsolePageViewModel vm = new ConsolePageViewModel()
            {
                Status = EnumExtensions.GetEnumDescription(_proxyService.GetStatus()),
                Version = version,
            };
            return View(vm);
        }
    }
}
