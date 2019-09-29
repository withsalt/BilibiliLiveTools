using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bilibili.Model.User.LoginResult
{
    public class CookiesItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("http_only")]
        public int HttpOnly { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Expires { get; set; }
    }
}
