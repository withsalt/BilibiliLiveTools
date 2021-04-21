using BilibiliLiver.Model.Interface;

namespace BilibiliLiver.Model.Live
{
    public class StopLiveDataInfo : IResultData
    {
        /// <summary>
        /// 
        /// </summary>
        public int Change { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Status { get; set; }
    }
}
