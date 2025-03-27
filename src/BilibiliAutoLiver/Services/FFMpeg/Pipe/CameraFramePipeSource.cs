using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using FFMpegCore.Pipes;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace BilibiliAutoLiver.Services.FFMpeg.Pipe
{
    public class CameraFramePipeSource : IPipeSource
    {
        public readonly Channel<SKBitmap> _frameChannel;
        private readonly Stopwatch _frameCounter = new Stopwatch();
        private readonly ILogger _logger;

        public CameraFramePipeSource(Channel<SKBitmap> frameChannel, ILogger logger)
        {
            _frameChannel = frameChannel ?? throw new ArgumentNullException(nameof(frameChannel), "The frame channel must be initialized first.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger must be initialized first.");
        }

        public string GetStreamArguments()
        {
            return "";
        }

        public async Task WriteAsync(Stream outputStream, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && await this._frameChannel.Reader.WaitToReadAsync(cancellationToken))
                {
                    SKBitmap latestBitmap = null;

                    // 取出所有可用帧，只保留最后一帧
                    while (this._frameChannel.Reader.TryRead(out SKBitmap bitmap))
                    {
                        if (this._frameChannel.Reader.Count > 2)
                        {
                            _logger.LogWarning("帧队列堆积，丢弃帧...");
                        }

                        latestBitmap?.Dispose();
                        latestBitmap = bitmap;
                    }

                    try
                    {
                        if (latestBitmap != null)
                        {
                            await outputStream.WriteAsync(latestBitmap.Bytes, cancellationToken).ConfigureAwait(false);
                            _frameCounter.Restart();
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    finally
                    {
                        latestBitmap?.Dispose();
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while writing camera frame pipe source.");
            }
        }
    }
}
