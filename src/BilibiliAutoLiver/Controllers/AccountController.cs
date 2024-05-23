using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models;
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
        private readonly IBilibiliCookieService _cookieService;
        private readonly IPushStreamProxyService _pushStreamProxyService;

        public AccountController(ILogger<AccountController> logger
            , IMemoryCache cache
            , IBilibiliAccountApiService accountService
            , IBilibiliCookieService cookieService
            , IPushStreamProxyService pushStreamProxyService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _cookieService = cookieService ?? throw new ArgumentNullException(nameof(cookieService));
            _pushStreamProxyService = pushStreamProxyService ?? throw new ArgumentNullException(nameof(pushStreamProxyService));
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            if (_accountService.GetLoginStatus())
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
            _accountService.SetLoginStatus(false);
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
            if (_accountService.TryGetQrCodeLoginStatus(out _))
            {
                return Task.FromResult("正在通过扫描二维码登录");
            }
            _ = _accountService.LoginByQrCode();
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
                await _accountService.RefreshCookie();
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
            if (_accountService.GetLoginStatus())
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
