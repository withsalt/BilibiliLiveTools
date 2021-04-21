using BilibiliLiver.Model.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BilibiliLiver.Model.Live
{
    public class RealAddressDataInfo : IResultData
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("current_quality")]
        public int CurrentQuality { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("accept_quality")]
        public List<string> AcceptQuality { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("current_qn")]
        public int CurrentQn { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("quality_description")]
        public List<QualityDescriptionItem> QualityDescription { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<DurlItem> Durl { get; set; }
    }
}
