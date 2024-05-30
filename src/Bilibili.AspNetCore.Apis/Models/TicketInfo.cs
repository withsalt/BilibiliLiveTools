using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bilibili.AspNetCore.Apis.Models
{
    public class TicketInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public string ticket { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int created_at { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int ttl { get; set; }
    }
}
