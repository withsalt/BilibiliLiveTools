﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BilibiliLiveCommon.Config
{
    public class CookiesConfig
    {
        public IEnumerable<CookieHeaderValue> Cookies { get; set; }

        public string RefreshToken { get; set; }

        /// <summary>
        /// 是否过期
        /// </summary>
        public bool IsExpired
        {
            get
            {
                if (this.Cookies == null) return true;
                if (this.Cookies.Any(p => p.Cookies.Any(q => q.Name == "bili_jct") && p.Expires <= DateTime.UtcNow)) return true;
                return false;
            }
        }

        public bool TryGetValue(string key, out string outValue)
        {
            outValue = null;
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            if (Cookies?.Any() != true) return false;
            foreach (var item in Cookies)
            {
                if (item.Expires < DateTime.UtcNow)
                {
                    continue;
                }
                if (!item.Cookies.Any(p => p.Name == key))
                {
                    continue;
                }
                string result = item[key].Value;
                if (string.IsNullOrWhiteSpace(result))
                {
                    continue;
                }
                outValue = result.Trim();
                return true;
            }
            return false;
        }

        public string GetCookieString()
        {
            if (Cookies?.Any() != true) return null;
            var cookies = this.Cookies.SelectMany(p => p.Cookies.Select(q => $"{q.Name}={q.Value}")).ToList();
            if (cookies?.Any() != true) return null;
            return string.Join("; ", cookies);
        }
    }
}