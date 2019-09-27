using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bilibili.Model.Live.StartLiveInfo
{
    public class ProtocolsItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string Protocol { get; set; }

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
