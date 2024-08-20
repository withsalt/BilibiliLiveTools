using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models;
using BilibiliAutoLiver.Config;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Services.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IMemoryCache _cache;
        private readonly IBilibiliAccountApiService _accountService;
        private readonly IBilibiliCookieService _cookie;
        private readonly IPushStreamProxyService _pushStreamProxyService;
        private readonly ILocalLockService _lockService;

        public AccountController(ILogger<AccountController> logger
            , IMemoryCache cache
            , IBilibiliAccountApiService accountService
            , IBilibiliCookieService cookie
            , IPushStreamProxyService pushStreamProxyService
            , ILocalLockService lockService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _cookie = cookie ?? throw new ArgumentNullException(nameof(cookie));
            _pushStreamProxyService = pushStreamProxyService ?? throw new ArgumentNullException(nameof(pushStreamProxyService));
            _lockService = lockService ?? throw new ArgumentNullException(nameof(lockService));
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            if (_accountService.IsLogged())
            {
                UserInfo userInfo = await _accountService.GetUserInfo();
                if (userInfo == null)
                {
                    return await Logout();
                }
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userInfo.Uname),
                    new Claim("Mid", userInfo.Mid.ToString()),
                    new Claim(ClaimTypes.Role, "Administrator"),
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    ExpiresUtc = DateTime.MaxValue,
                };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("手动操作退出登录。");
            //清除登录状态
            _accountService.Logout();
            //停止推流
            await _pushStreamProxyService.Stop();
            //登出
            await HttpContext.SignOutAsync();
            //重新请求登录二维码
            _ = await LoginByQrCode();
            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// 请求通过二维码登录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public Task<string> LoginByQrCode()
        {
            if (_lockService.IsLocked(CacheKeyConstant.LOGING_STATUS_CACHE_KEY))
            {
                return Task.FromResult("别着急，再等等...");
            }
            if (_accountService.TryGetQrCodeLoginStatus(out _))
            {
                return Task.FromResult("正在通过扫描二维码登录");
            }
            if (_lockService.Lock(CacheKeyConstant.QRCODE_LOGIN_STATUS_CACHE_KEY, TimeSpan.FromSeconds(300)))
            {
                try
                {
                    _ = _accountService.LoginByQrCode();
                }
                finally
                {
                    _lockService.UnLock(CacheKeyConstant.QRCODE_LOGIN_STATUS_CACHE_KEY);
                }
            }
            else
            {
                return Task.FromResult("正在通过扫描二维码登录");
            }
            return Task.FromResult("Ok");
        }

        /// <summary>
        /// 刷新Cookie
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Refresh()
        {
            try
            {
                await _cookie.RefreshCookie();
                _logger.LogInformation("强制重新刷新Cookie成功。");
                return Content("刷新成功");
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public BilibiliAccountLoginStatus Status()
        {
            //第一次开启应用，正在尝试通过Cookie登录
            if (_cache.Get<bool>(CacheKeyConstant.LOGING_STATUS_CACHE_KEY) == true)
            {
                return new BilibiliAccountLoginStatus()
                {
                    Status = AccountLoginStatus.Waiting,
                };
            }

            if (_accountService.IsLogged())
            {
                return new BilibiliAccountLoginStatus()
                {
                    Status = AccountLoginStatus.Logged,
                    RedirectUrl = Url.Action("Index", "Home")
                };
            }
            else
            {
                BilibiliAccountLoginStatus status = new BilibiliAccountLoginStatus();
                if (_accountService.TryGetQrCodeLoginStatus(out QrCodeLoginStatus loginStatus))
                {
                    status.Status = AccountLoginStatus.Logging;
                    status.QrCodeStatus = loginStatus;
                }
                else
                {
                    //未登录
                    status.Status = AccountLoginStatus.NotLogin;
                    status.QrCodeStatus = new QrCodeLoginStatus()
                    {
                        IsLogged = false,
                    };
                }
                return status;
            }
        }
    }
}
