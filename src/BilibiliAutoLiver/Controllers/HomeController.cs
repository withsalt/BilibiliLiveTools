using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models;
using BilibiliAutoLiver.Config;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMemoryCache _cache;
        private readonly IBilibiliAccountApiService _accountService;
        private readonly IBilibiliCookieService _cookieService;
        private readonly IBilibiliLiveApiService _liveApiService;

        public HomeController(ILogger<HomeController> logger
            , IMemoryCache cache
            , IBilibiliAccountApiService accountService
            , IBilibiliCookieService cookieService
            , IBilibiliLiveApiService liveApiService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _cookieService = cookieService ?? throw new ArgumentNullException(nameof(cookieService));
            _liveApiService = liveApiService ?? throw new ArgumentNullException(nameof(liveApiService));
        }

        public async Task<IActionResult> Index()
        {
            IndexPageStatusDto pageSatus = await Status();
            ViewData[nameof(IndexPageStatusDto)] = pageSatus;
            return View();
        }

        [HttpPost]
        public async Task<IndexPageStatusDto> Status()
        {
            IndexPageStatusDto pageSatus = new IndexPageStatusDto();
            if (_accountService.TryGetQrCodeLoginStatus(out var loginStatus))
            {
                pageSatus.LoginStatus = loginStatus;
                pageSatus.LoginStatus.IsLogged = false;
            }
            else
            {
                if (_cookieService.HasCookie())
                {
                    pageSatus = await _cache.GetOrCreateAsync<IndexPageStatusDto>(CacheKeyConstant.INDEX_PAGE_CACHE_KEY, async (cacheEntry) =>
                    {
                        cacheEntry.AbsoluteExpiration = DateTime.UtcNow.AddMinutes(30);

                        var cachePageStatus = new IndexPageStatusDto();
                        cachePageStatus.LoginStatus = new QrCodeLoginStatus()
                        {
                            IsLogged = true
                        };
                        cachePageStatus.UserInfo = await _accountService.GetUserInfo();
                        cachePageStatus.LiveRoomInfo = await _liveApiService.GetLiveRoomInfo();
                        return cachePageStatus;
                    });
                }
                else
                {
                    pageSatus.LoginStatus = new QrCodeLoginStatus();
                    pageSatus.LoginStatus.IsLogged = false;
                }
            }
            return pageSatus;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
