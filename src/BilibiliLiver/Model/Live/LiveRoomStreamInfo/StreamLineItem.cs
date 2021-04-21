using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BilibiliLiver.Model.Live
{
    public class StreamLineItem
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("cdn_name")]
        public string CdnName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Checked { get; set; }

        /// <summary>
        /// 默认线路
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Src { get; set; }
    }
}
