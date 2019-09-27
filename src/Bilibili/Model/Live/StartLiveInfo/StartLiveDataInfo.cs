using Bilibili.Model.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bilibili.Model.Live.StartLiveInfo
{
    public class StartLiveDataInfo : IResultData
    {
        /// <summary>
        /// 
        /// </summary>
        public int Change { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("room_type")]
        public int RoomType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// 
        public Rtmp Rtmp { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<ProtocolsItem> Protocols { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("try_time")]
        public string TryTime { get; set; }

        /// <summary>
        /// 直播间ID
        /// </summary>
        public string RoomId { get; set; }
    }
}
