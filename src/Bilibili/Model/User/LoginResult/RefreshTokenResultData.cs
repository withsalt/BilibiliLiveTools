using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bilibili.Model.User.LoginResult
{
    class RefreshTokenResultData
    {
        /// <summary>
        /// 
        /// </summary>
        public string Mid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
