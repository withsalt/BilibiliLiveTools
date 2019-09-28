using Bilibili.Model.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bilibili.Model.Live.StopLiveInfo
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
