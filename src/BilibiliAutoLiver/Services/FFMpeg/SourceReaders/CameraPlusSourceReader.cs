using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using BilibiliAutoLiver.Models;
using BilibiliAutoLiver.Models.Settings;
using BilibiliAutoLiver.Plugin.Base;
using BilibiliAutoLiver.Services.FFMpeg.DeviceProviders;
using BilibiliAutoLiver.Services.FFMpeg.Pipe;
using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace BilibiliAutoLiver.Services.FFMpeg.SourceReaders
{
    public class CameraPlusSourceReader : BaseSourceReader
    {
        private ICameraDeviceProvider DeviceProvider { get; }
        private IPipeContainer PipeContainer { get; }
        private CameraFramePipeSource PipeSource { get; }

        private Queue<BufferFrame> frameQueue = new Queue<BufferFrame>();
        private readonly int _frameRate = 30;

        public CameraPlusSourceReader(LiveSettings settings, string rtmpAddr, ILogger logger, IPipeContainer pipeContainer) : base(settings, rtmpAddr, logger)
        {
            this.PipeContainer = pipeContainer;
            this.DeviceProvider = new CameraDeviceProvider(settings.V2.Input.VideoSource, OnFrameArrived);
            this.DeviceProvider.Start();
            this.PipeSource = new CameraFramePipeSource(frameQueue);
        }

        private void OnFrameArrived(BufferFrame frame)
        {
            try
            {
                if (frameQueue.Count > _frameRate * 2)
                {
                    _logger.LogWarning("帧队列堆积，丢弃...");
                    while (frameQueue.TryDequeue(out var p))
                    {
                        p?.Dispose();
                    }
                }
                if (frame.Bitmap == null)
                {
                    return;
                }
                var pipes = PipeContainer.Get();
                if (pipes.Any())
                {
                    SKBitmap tmp = frame.Bitmap;
                    foreach (var item in pipes)
                    {
                        try
                        {
                            tmp = item.Process(tmp);
                            if (tmp == null)
                            {
                                _logger.LogWarning($"插件【{item.Name}】处理视频帧异常：返回帧数据为空");
                                tmp = frame.Bitmap;
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"插件【{item.Name}】处理视频帧异常：{ex.Message}");
                        }
                    }
                    frame.Bitmap = tmp;
                }
                if (frame.Bitmap == null)
                {
                    _logger.LogWarning($"帧数据由插件处理后为空。");
                    return;
                }
                frameQueue.Enqueue(frame);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"处理视频帧异常：{ex.Message}");
            }
        }

        public override ISourceReader WithInputArg()
        {
            GetVideoInputArg();
            GetAudioInputArg();
            return this;
        }

        public override FFMpegArgumentProcessor WithOutputArg()
        {
            if (FFMpegArguments == null)
                throw new Exception("请先指定输入参数");
            var rt = FFMpegArguments.OutputToUrl(RtmpAddr, opt =>
            {
                WithCommonOutputArg(opt);
            });
            return rt;
        }

        private void GetVideoInputArg()
        {
            this.FFMpegArguments = FFMpegArguments.FromPipeInput(this.PipeSource, opt =>
            {
                //opt.WithCustomArgument("-thread_queue_size 1024");
                opt.ForceFormat("rawvideo");
                opt.ForcePixelFormat("bgra");
                opt.WithFramerate(_frameRate);
                opt.Resize(this.DeviceProvider.Size.Width, this.DeviceProvider.Size.Height);

                WithMuteArgument(opt);
            });
        }

        public override void Dispose()
        {
            if (this.DeviceProvider != null)
            {
                this.DeviceProvider.Dispose();
            }
            if (frameQueue != null && frameQueue.Count > 0)
            {
                while (frameQueue.TryDequeue(out var p))
                {
                    p?.Dispose();
                }
            }
        }
    }
}
