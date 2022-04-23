using System.Collections.Generic;

namespace BilibiliLiveCommon.Model
{
    public class LiveAreaItem
    {
        /// <summary>
        /// 
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int parent_id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string old_area_id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string act_id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string pk_status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int hot_status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string lock_status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string pic { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string complex_area_name { get; set; }

        /// <summary>
        /// 网游
        /// </summary>
        public string parent_name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int area_type { get; set; }

        public List<LiveAreaItem> list { get; set; }
    }
}
