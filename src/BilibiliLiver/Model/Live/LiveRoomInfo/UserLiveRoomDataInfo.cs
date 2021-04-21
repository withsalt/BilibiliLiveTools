using BilibiliLiver.Model.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BilibiliLiver.Model.Live
{
    public class UserLiveRoomDataInfo : IResultData
    {
        /// <summary>
        /// 
        /// </summary>
        public string Uid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("room_id")]
        public int RoomId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("short_id")]
        public int ShortId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Attention { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Online { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("is_portrait")]
        public string IsPortrait { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("live_status")]
        public int LiveStatus { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("area_id")]
        public int AreaId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("parent_area_id")]
        public int ParentAreaId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("parent_area_name")]
        public string ParentAreaName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("old_area_id")]
        public int OldAreaId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Background { get; set; }

        /// <summary>
        /// 小金鱼啦~
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("user_cover")]
        public string UserCover { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string KeyFrame { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("is_strict_room")]
        public string IsStrictRoom { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("live_time")]
        public string LiveTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("is_anchor")]
        public int IsAnchor { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("room_silent_type")]
        public string RoomSilentType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("room_silent_level")]
        public int RoomSilentLevel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("room_silent_second")]
        public int RoomSilentSecond { get; set; }

        /// <summary>
        /// 萌宠
        /// </summary>
        [JsonProperty("area_name")]
        public string AreaName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Pendants { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("area_pendants")]
        public string AreaPendants { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("hot_words")]
        public List<string> HotWords { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("hot_words_status")]
        public int HotWordsStatus { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Verify { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("up_session")]
        public string UpSession { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("pk_status")]
        public int PkStatus { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("pk_id")]
        public int PkId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("battle_id")]
        public int BattleId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("allow_change_area_time")]
        public int AllowChangeAreaTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("allow_upload_cover_time")]
        public int AllowUploadCoverTime { get; set; }
    }
}
