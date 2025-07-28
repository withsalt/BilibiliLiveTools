using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bilibili.AspNetCore.Apis.Models
{
    public class BilibiliAppKey
    {
        /// <summary>
        /// 获取或设置B站AppKey
        /// </summary>
        public string AppKey { get; set; }

        /// <summary>
        /// 获取或设置B站AppSecret
        /// </summary>
        public string AppSecret { get; set; }

        public string Platform { get; set; }

        public int NeuronAppId { get; set; }
    }
}
