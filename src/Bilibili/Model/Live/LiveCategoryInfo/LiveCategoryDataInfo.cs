using System;
using System.Collections.Generic;
using System.Text;

namespace Bilibili.Model.Live.LiveCategoryInfo
{
    public class LiveCategoryDataInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<BigCategoryItem> Data { get; set; }
    }
}
