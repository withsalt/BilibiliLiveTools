using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharp.QrCode.Image;

namespace BilibiliLiveCommon.Utils
{
    public class QrCode
    {
        public static byte[] Generate(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentNullException("text");
            var qrCode = new SkiaSharp.QrCode.Image.QrCode(text, new Vector2Slim(512, 512), SKEncodedImageFormat.Png);
            using (MemoryStream ms = new MemoryStream())
            {
                qrCode.GenerateImage(ms, true, SkiaSharp.QrCode.ECCLevel.M);
                return ms.ToArray();
            }
        }
    }
}
