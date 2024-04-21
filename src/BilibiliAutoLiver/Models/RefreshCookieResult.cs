using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliAutoLiver.Models
{
    public class RefreshCookieResult
    {
        public int status { get; set; }
        public string message { get; set; }
        public string refresh_token { get; set; }
    }
}
