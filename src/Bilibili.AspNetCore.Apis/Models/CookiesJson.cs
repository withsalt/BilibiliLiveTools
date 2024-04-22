using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace Bilibili.AspNetCore.Apis.Models
{
    public class CookiesJson
    {
        public List<string> Cookies { get; set; }

        public string RefreshToken { get; set; }

        public CookiesJson()
        {

        }

        public CookiesJson(CookiesConfig cookiesConfig)
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

        public CookiesConfig ConvertTo()
        {
            List<CookieHeaderValue> cookies = new List<CookieHeaderValue>();
            foreach (var item in Cookies)
            {
                if (CookieHeaderValue.TryParse(item, out var value))
                {
                    cookies.Add(value);
                }
                else
                {
                    throw new Exception($"Try parse \"{item}\" to cookie header value failed.");
                }
            }
            CookiesConfig cookiesConfig = new CookiesConfig()
            {
                Cookies = cookies,
                RefreshToken = RefreshToken
            };
            return cookiesConfig;
        }
    }
}
