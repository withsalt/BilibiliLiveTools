using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FFMpegCore.Pipes;

namespace BilibiliAutoLiver.Services.FFMpeg.PipeSource
{
    public class VideoFramePipeSource : IPipeSource
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        /// <summary>
        /// 输入帧数
        /// </summary>
        public double FrameRate { get; set; }

        /// <summary>
        /// 帧队列
        /// </summary>
        public Queue<byte[]> FrameQueue { get; set; }

        private byte[] _lastData = null;
        private readonly Stopwatch _frameCounter = new Stopwatch();

        public VideoFramePipeSource(Queue<byte[]> frameQueue, int width, int height, double frameRate)
        {
            Width = width;
            Height = height;
            FrameRate = frameRate;
            if (FrameRate <= 0)
            {
                throw new ArgumentException("Frame rate can not less than 0.");
            }
            FrameQueue = frameQueue;
            if (FrameQueue == null)
            {
                throw new ArgumentNullException(nameof(frameQueue), "The bytes queue must be initialized first.");
            }
        }

        public string GetStreamArguments()
        {
            return $"-f image2pipe -r {FrameRate.ToString(CultureInfo.InvariantCulture)} -s {Width}x{Height}";
        }

        public async Task WriteAsync(Stream outputStream, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (FrameQueue.TryDequeue(out var data) && data != null && data.Length > 0)
                    {
                        await outputStream.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);

                        _frameCounter.Restart();
                        _lastData = new byte[data.Length];
                        Array.ConstrainedCopy(data, 0, _lastData, 0, data.Length);
                    }
                    else
                    {
                        if (_frameCounter.ElapsedMilliseconds > 100 && _lastData != null && _lastData.Length > 0)
                        {
                            //如果超过100ms还没有视频帧，则补上一帧
                            await outputStream.WriteAsync(_lastData, 0, _lastData.Length, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            await Task.Delay(10, cancellationToken);
                        }
                    }
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Write byte data to outputStream failed. {ex.Message}");
                }
            }
        }
    }
}
