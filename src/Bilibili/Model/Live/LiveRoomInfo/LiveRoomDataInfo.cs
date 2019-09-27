using Bilibili.Model.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bilibili.Model.Live.LiveRoomInfo
{
    public class LiveRoomDataInfo : IResultData
    {
        /// <summary>
        /// 
        /// </summary>
        public int Achieves { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public UserInfo UserInfo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string RoomId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public UserCoinIfo UserCoinIfo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string VipViewStatus { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Discount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("svip_endtime")]
        public int SvipEndtime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("vip_endtime")]
        public int VipEndTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("year_price")]
        public int YearPrice { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("month_price")]
        public int MonthPrice { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int LiveTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Master Master { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int San { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Count Count { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("show_guild_menu")]
        public string ShowGuildMenu { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("sign_anchor")]
        public SignAnchor SignAnchor { get; set; }
    }
}
