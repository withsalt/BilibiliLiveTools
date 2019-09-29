using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bilibili.Model.User.LoginResult
{
    public class LoginResultData
    {
        /// <summary>
        /// 
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Mid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("token_info")]
        public TokenInfo TokenInfo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("cookie_info")]
        public CookieInfo CookieInfo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> SSO { get; set; }
    }
}
