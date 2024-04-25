using System;
using System.IO;
using SkiaSharp;
using SkiaSharp.QrCode.Image;

namespace Bilibili.AspNetCore.Apis.Models
{
    public class QrCodeUrl
    {
        public string qrcode_key { get; set; }

        public string url { get; set; }

        public byte[] GetBytes()
        {
            if (string.IsNullOrWhiteSpace(this.url)) throw new ArgumentNullException("url");
            var qrCode = new SkiaSharp.QrCode.Image.QrCode(this.url, new Vector2Slim(512, 512), SKEncodedImageFormat.Png);
            using (MemoryStream ms = new MemoryStream())
            {
                qrCode.GenerateImage(ms, true, SkiaSharp.QrCode.ECCLevel.M);
                return ms.ToArray();
            }
        }
    }
}
