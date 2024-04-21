using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliAutoLiver.Model
{
    public class ConfirmRefreshModel
    {
        public string csrf { get; set; }
        public string refresh_token { get; set; }
    }
}
