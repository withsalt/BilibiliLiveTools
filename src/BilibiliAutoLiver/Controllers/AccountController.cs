using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IBilibiliAccountApiService _accountService;
        private readonly IBilibiliCookieService _cookieService;

        public AccountController(ILogger<AccountController> logger
            , IBilibiliAccountApiService accountService
            , IBilibiliCookieService cookieService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _cookieService = cookieService ?? throw new ArgumentNullException(nameof(cookieService));
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Administrator"),
                new Claim("FullName", "222"),
                new Claim(ClaimTypes.Role, "Administrator"),
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTime.MaxValue,
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
            return Content("登录成功");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _cookieService.RemoveCookie();

            await HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Account");
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

        /// <summary>
        /// 重新登录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ReLogin()
        {
            await _cookieService.RemoveCookie();
            _ = _accountService.LoginByQrCode();

            return Content("重新扫码登录，请返回首页进行扫码登录。");
        }
    }
}
