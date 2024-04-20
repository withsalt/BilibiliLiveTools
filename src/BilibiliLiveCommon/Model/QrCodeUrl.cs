using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BilibiliLiveCommon.Utils;

namespace BilibiliLiveCommon.Model
{
    public class QrCodeUrl
    {
        public string qrcode_key { get; set; }

        public string url { get; set; }

        public byte[] GetBytes()
        {
            return QrCode.Generate(this.url);
        }
    }
}
