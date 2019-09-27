using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bilibili.Model.Live.StartLiveInfo
{
    public class Rtmp
    {
        /// <summary>
        /// 
        /// </summary>
        public string Addr { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("new_link")]
        public string NewLink { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Provider { get; set; }
    }
}
