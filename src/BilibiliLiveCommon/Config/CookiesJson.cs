﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BilibiliLiveCommon.Config
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
            this.Cookies = new List<string>();
            foreach (var item in cookiesConfig.Cookies)
            {
                this.Cookies.Add(item.ToString());
            }
            this.RefreshToken = cookiesConfig.RefreshToken;
        }

        public CookiesConfig ConvertTo()
        {
            List<CookieHeaderValue> cookies = new List<CookieHeaderValue>();
            foreach (var item in this.Cookies)
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
                RefreshToken = this.RefreshToken
            };
            return cookiesConfig;
        }
    }
}