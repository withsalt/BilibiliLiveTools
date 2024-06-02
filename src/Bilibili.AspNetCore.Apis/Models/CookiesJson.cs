using System;
using System.Collections.Generic;
using System.Linq;

namespace Bilibili.AspNetCore.Apis.Models
{
    public class CookiesJson
    {
        public List<string> Cookies { get; set; }

        public string RefreshToken { get; set; }

        public CookiesJson()
        {

        }

        public CookiesJson(CookiesData cookiesConfig)
        {
            if (cookiesConfig == null || cookiesConfig.Cookies?.Any() != true || string.IsNullOrWhiteSpace(cookiesConfig.RefreshToken))
            {
                throw new ArgumentNullException("Cookie参数不完整！");
            }
            Cookies = new List<string>();
            foreach (var item in cookiesConfig.Cookies)
            {
                Cookies.Add(item.ToString());
            }
            RefreshToken = cookiesConfig.RefreshToken;
        }
    }
}
