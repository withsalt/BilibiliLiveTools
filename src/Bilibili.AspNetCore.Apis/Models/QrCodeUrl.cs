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
            if (string.IsNullOrWhiteSpace(this.url)) 
                throw new ArgumentNullException("url");

            return QRCodeImageBuilder.GetImageBytes(this.url, SKEncodedImageFormat.Png, SkiaSharp.QrCode.ECCLevel.M, 512);
        }
    }
}
