using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLiver.Services
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
