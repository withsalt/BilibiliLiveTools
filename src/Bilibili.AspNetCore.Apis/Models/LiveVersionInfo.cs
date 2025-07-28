using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bilibili.AspNetCore.Apis.Models
{
    public class LiveVersionInfo
    {
        public string curr_version { get; set; }

        public int build { get; set; }

        public string instruction { get; set; } 

        public string file_size { get; set; }

        public string file_md5 { get; set; }

        public string content { get; set; }

        public string download_url { get; set; }

        public int hdiffpatch_switch { get; set; }
    }
}
