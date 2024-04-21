using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliAutoLiver.Model
{
    public class LoginStatusCacheInfo
    {
        public bool IsLogged { get; set; }

        public bool IsScaned { get; set; }

        public string QrCode { get; set; }

        public int QrCodeEffectiveTime { get; set; }

        public int Index { get; set; }
    }
}
