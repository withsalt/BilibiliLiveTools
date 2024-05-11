using System;
using BilibiliAutoLiver.Plugin.Base;
using SkiaSharp;

namespace BilibiliAutoLiver.Plugin.YoloAI
{
    public class YoloPipeProcess : IPipeProcess
    {
        public int Index { get; }

        public string Name { get; } = "YoloV8检测";

        public YoloPipeProcess()
        {
            this.Index = 99;
        }

        public SKBitmap Process(SKBitmap bitmap)
        {
            return bitmap;
        }
    }
}
