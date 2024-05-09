using System.Collections.Generic;

namespace Bilibili.AspNetCore.Apis.Models
{
    public class LiveRoomInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public long uid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public long room_id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int short_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int attention { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int online { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string is_portrait { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int live_status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int area_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int parent_area_id { get; set; }
        /// <summary>
        /// 知识
        /// </summary>
        public string parent_area_name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int old_area_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string background { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string user_cover { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string keyframe { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string is_strict_room { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string live_time { get; set; }
        /// <summary>
        /// realme,旗舰手机,真我手机,真我GT Neo6,新品发布会
        /// </summary>
        public string tags { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int is_anchor { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string room_silent_type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int room_silent_level { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int room_silent_second { get; set; }
        /// <summary>
        /// 科技·科学
        /// </summary>
        public string area_name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string pendants { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string area_pendants { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> hot_words { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int hot_words_status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string verify { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public New_pendants new_pendants { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string up_session { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int pk_status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int pk_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int battle_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int allow_change_area_time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int allow_upload_cover_time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Studio_info studio_info { get; set; }

        /// <summary>
        /// 是否处于直播中
        /// </summary>
        public bool is_living
        {
            get
            {
                return live_status == 1 || live_status == 2;
            }
        }
    }

    public class Frame
    {
        /// <summary>
        /// 
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string value { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int position { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string desc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int area { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int area_old { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string bg_color { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string bg_pic { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string use_old_area { get; set; }
    }

    public class Badge
    {
        /// <summary>
        /// 
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int position { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string value { get; set; }
        /// <summary>
        /// 真我手机官方账号
        /// </summary>
        public string desc { get; set; }
    }

    public class Mobile_frame
    {
        /// <summary>
        /// 
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string value { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int position { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string desc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int area { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int area_old { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string bg_color { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string bg_pic { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string use_old_area { get; set; }
    }

    public class New_pendants
    {
        /// <summary>
        /// 
        /// </summary>
        public Frame frame { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Badge badge { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Mobile_frame mobile_frame { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string mobile_badge { get; set; }
    }

    public class Studio_info
    {
        /// <summary>
        /// 
        /// </summary>
        public int status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> master_list { get; set; }
    }
}
