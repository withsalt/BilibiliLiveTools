using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models;
using BilibiliLiveCommon.Config;
using BilibiliLiveCommon.Model;
using BilibiliLiveCommon.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMemoryCache _cache;
        private readonly IBilibiliAccountService _accountService;
        private readonly IBilibiliCookieService _cookieService;

        public HomeController(ILogger<HomeController> logger
            , IMemoryCache cache
            , IBilibiliAccountService accountService
            , IBilibiliCookieService cookieService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _cookieService = cookieService ?? throw new ArgumentNullException(nameof(cookieService));
        }

        public async Task<IActionResult> Index()
        {
            IndexPageStatus pageSatus = await Status();
            ViewData[nameof(IndexPageStatus)] = pageSatus;
            return View();
        }

        [HttpPost]
        public async Task<IndexPageStatus> Status()
        {
            IndexPageStatus pageSatus = new IndexPageStatus();
            LoginStatusCacheInfo loginInfo = _cache.Get<LoginStatusCacheInfo>(CacheKeyConstant.LOGIN_STATUS_CACHE_KEY);
            if (loginInfo != null)
            {
                pageSatus.LoginStatus = loginInfo;
                pageSatus.LoginStatus.IsLogged = false;
            }
            else
            {
                if (_cookieService.HasCookie())
                {
                    pageSatus = await _cache.GetOrCreateAsync<IndexPageStatus>(CacheKeyConstant.INDEX_PAGE_CACHE_KEY, async (cacheEntry) =>
                    {
                        cacheEntry.AbsoluteExpiration = DateTime.UtcNow.AddMinutes(30);

                        var cachePageSatus = new IndexPageStatus();
                        cachePageSatus.LoginStatus = new LoginStatusCacheInfo()
                        {
                            IsLogged = true
                        };
                        cachePageSatus.UserInfo = await _accountService.GetUserInfo();

                        return cachePageSatus;
                    });
                }
                else
                {
                    pageSatus.LoginStatus = new LoginStatusCacheInfo();
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
