using System;
using System.Collections.Generic;
using System.Text;

namespace Bilibili.Model.Live.LiveCategoryInfo
{
    public class BigCategoryItem
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<SmallCategoryItem> List { get; set; }
    }
}
