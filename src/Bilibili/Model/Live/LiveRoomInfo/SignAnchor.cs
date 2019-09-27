using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bilibili.Model.Live.LiveRoomInfo
{
    public class SignAnchor
    {
        /// <summary>
        /// 
        /// </summary>
        public int Atatus { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("start_date")]
        public string StartDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("end_date")]
        public string EndDate { get; set; }
    }
}
