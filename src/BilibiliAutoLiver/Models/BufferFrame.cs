using System;
using SkiaSharp;

namespace BilibiliAutoLiver.Models
{
    public sealed class BufferFrame : IDisposable
    {
        public BufferFrame(SKBitmap bitmap, int frameCount, long frameIndex, TimeSpan timestamp)
        {
            Bitmap = bitmap;
            FrameCount = frameCount;
            FrameIndex = frameIndex;
            Timestamp = timestamp;
        }

        public SKBitmap Bitmap { get; set; }

        public int FrameCount { get; set; }

        public long FrameIndex { get; set; }

        public TimeSpan Timestamp { get; set; }

        public void Dispose()
        {
            if (Bitmap != null)
            {
                Bitmap.Dispose();
            }
        }
    }
}
