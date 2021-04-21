using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BilibiliLiver.Model.Live
{
    public class UserInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public int Uid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Uname { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Face { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Identification { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("mobile_verify")]
        public int MobileVerify { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("platform_user_level")]
        public int PlatformUserLevel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("vip_type")]
        public int VipType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Gender { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("official_verify")]
        public OfficialVerify OfficialVerify { get; set; }
    }
}
