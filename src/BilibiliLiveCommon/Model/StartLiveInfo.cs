using System.Collections.Generic;

namespace BilibiliLiveCommon.Model
{
    public class StartLiveInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public int change { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string try_time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int room_type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string live_key { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string sub_session_key { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Rtmp rtmp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ProtocolsItem> protocols { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string qr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool need_face_auth { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string service_source { get; set; }
    }
}
