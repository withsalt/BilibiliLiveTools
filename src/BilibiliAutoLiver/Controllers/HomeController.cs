using System;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using BilibiliAutoLiver.Models.ViewModels;
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

        public HomeController(ILogger<HomeController> logger
            , IMemoryCache cache
            , IBilibiliAccountApiService accountService
            , IBilibiliCookieService cookieService
            , IBilibiliLiveApiService liveApiService
            , IPushStreamProxyService proxyService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _cookieService = cookieService ?? throw new ArgumentNullException(nameof(cookieService));
            _liveApiService = liveApiService ?? throw new ArgumentNullException(nameof(liveApiService));
            _proxyService = proxyService ?? throw new ArgumentNullException(nameof(proxyService));
        }

        public async Task<IActionResult> Index()
        {
            var userInfo = await _accountService.GetUserInfo();

            return View(new HomeIndexPageViewModel()
            {
                UserInfo = userInfo,
            });
        }

        public IActionResult Console()
        {
            return View();
        }
    }
}
