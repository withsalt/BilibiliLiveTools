using System.Net.Http.Headers;

namespace BilibiliLiveCommon.Services.Interface
{
    public interface IBilibiliCookieService
    {
        void Init();

        string Get(bool force = false);

        CookieHeaderValue CookieDeserialize(string cookieText);

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
    }
}
