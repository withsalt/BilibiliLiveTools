using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Models;

namespace Bilibili.AspNetCore.Apis.Interface
{
    public interface IBilibiliCookieService
    {
        bool HasCookie();

        Task SaveCookie(IEnumerable<CookieHeaderValue> cookies, string refreshToken);

        Task RemoveCookie();

        string GetString(bool force = false);

        CookiesData GetCookies(bool force = false);

        /// <summary>
        /// 是否要过期了
        /// </summary>
        /// <param name="minHours"></param>
        /// <returns></returns>
        (bool, DateTimeOffset) WillExpired(int minHours = 24);

        /// <summary>
        /// 获取Csrf
        /// </summary>
        /// <returns></returns>
        string GetCsrf();

        /// <summary>
        /// 获取Userid
        /// </summary>
        /// <returns></returns>
        string GetUserId();

        /// <summary>
        /// 获取刷新token
        /// </summary>
        /// <returns></returns>
        string GetRefreshToken();

        /// <summary>
        /// 刷新Cookie
        /// </summary>
        /// <returns></returns>
        Task<bool> RefreshCookie();

        /// <summary>
        /// 是否需要刷新Cookie
        /// </summary>
        /// <returns></returns>
        Task<bool> CookieNeedToRefresh();
    }
}
