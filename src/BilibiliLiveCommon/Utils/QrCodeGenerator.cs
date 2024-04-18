using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharp.QrCode.Image;

namespace BilibiliLiveCommon.Utils
{
    public class QrCodeGenerator
    {
        public static MemoryStream Generate(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentNullException("text");
            var qrCode = new QrCode(text, new Vector2Slim(512, 512), SKEncodedImageFormat.Png);
            MemoryStream ms = new MemoryStream();
            qrCode.GenerateImage(ms, true, SkiaSharp.QrCode.ECCLevel.M);
            return ms;
        }
    }
}
