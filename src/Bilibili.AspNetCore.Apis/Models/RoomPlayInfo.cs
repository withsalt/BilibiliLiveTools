using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bilibili.AspNetCore.Apis.Models
{
    public class RoomPlayInfo
    {
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
        public long uid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool is_hidden { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool is_locked { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool is_portrait { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int live_status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int hidden_till { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int lock_till { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool encrypted { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool pwd_verified { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int live_time { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int room_shield { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> all_special_types { get; set; }

        /// <summary>
        /// 是否处于直播中
        /// </summary>
        public bool is_living
        {
            get
            {
                return live_status == 1;
            }
        }
    }
}
