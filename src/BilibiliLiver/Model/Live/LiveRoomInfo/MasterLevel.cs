using System;
using System.Collections.Generic;
using System.Text;

namespace BilibiliLiver.Model.Live
{
    public class MasterLevel
    {
        /// <summary>
        /// 
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<int> Current { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<int> Next { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Color { get; set; }
    }
}
