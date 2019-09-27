using Bilibili.Model.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bilibili.Model.Live.LiveRoomStreamInfo
{
    public class LiveRoomStreamDataInfo : IResultData
    {
        /// <summary>
        /// 
        /// </summary>
        public Rtmp Rtmp { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("stream_line")]
        public List<StreamLineItem> StreamLine { get; set; }
    }
}
