using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models;
using FFMpegCore.Pipes;
using SkiaSharp;

namespace BilibiliAutoLiver.Services.FFMpeg.Pipe
{
    public class CameraFramePipeSource : IPipeSource
    {
        /// <summary>
        /// 输入帧数
        /// </summary>
        public double FrameRate { get; set; }

        /// <summary>
        /// 帧队列
        /// </summary>
        public Queue<BufferFrame> FrameQueue { get; set; }

        private ReadOnlyMemory<byte> _lastData = new ReadOnlyMemory<byte>();
        private readonly Stopwatch _frameCounter = new Stopwatch();

        public CameraFramePipeSource(Queue<BufferFrame> frameQueue)
        {
            FrameQueue = frameQueue;
            if (FrameQueue == null)
            {
                throw new ArgumentNullException(nameof(frameQueue), "The bytes queue must be initialized first.");
            }
        }

        public string GetStreamArguments()
        {
            return "";
        }

        public async Task WriteAsync(Stream outputStream, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                BufferFrame frame = null;
                try
                {
                    if (FrameQueue.TryDequeue(out frame) && frame != null && frame.Bitmap?.Bytes.Length > 0)
                    {
                        _lastData = frame.Bitmap.Bytes.AsMemory(0, frame.Bitmap.Bytes.Length);
                        await outputStream.WriteAsync(_lastData, cancellationToken).ConfigureAwait(false);
                        _frameCounter.Restart();
                    }
                    else
                    {
                        if (_frameCounter.ElapsedMilliseconds > 100 && !_lastData.IsEmpty)
                        {
                            //如果超过100ms还没有视频帧，则补上一帧
                            await outputStream.WriteAsync(_lastData, cancellationToken).ConfigureAwait(false);
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
                finally
                {
                    if (frame != null) frame.Dispose();
                }
            }
        }
    }
}
