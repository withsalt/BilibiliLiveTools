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
    public class DeviceSourceReader : BaseSourceReader
    {
        private ICameraDeviceProvider DeviceProvider { get; }
        private IPipeContainer PipeContainer { get; }
        private CameraFramePipeSource PipeSource { get; }
        private Queue<BufferFrame> frameQueue = new Queue<BufferFrame>();

        private readonly int _frameRate = 30;

        public DeviceSourceReader(LiveSettings settings, string rtmpAddr, ILogger logger, IPipeContainer pipeContainer) : base(settings, rtmpAddr, logger)
        {
            this.PipeContainer = pipeContainer;
            this.DeviceProvider = new CameraDeviceProvider(settings.V2.Input.VideoSource, OnFrameArrived);
            this.DeviceProvider.Start();
            this.PipeSource = new CameraFramePipeSource(frameQueue, this.DeviceProvider.Size.Width, this.DeviceProvider.Size.Height, _frameRate);
        }

        private void OnFrameArrived(BufferFrame frame)
        {
            try
            {
                if (frameQueue.Count > _frameRate * 2)
                {
                    _logger.LogWarning("帧队列堆积，丢弃...");
                    frameQueue.Clear();
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
            if (FFMpegArguments == null) throw new Exception("请先指定输入参数");
            var rt = FFMpegArguments.OutputToUrl(RtmpAddr, opt =>
            {
                //禁用视频中的音频
                if (!string.IsNullOrEmpty(videoMuteMapOpt))
                {
                    opt.WithCustomArgument(videoMuteMapOpt);
                }
                //禁用音频中的视频
                if (!string.IsNullOrEmpty(audioMuteMapOpt))
                {
                    opt.WithCustomArgument(audioMuteMapOpt);
                }
                //音频编码
                if (HasAudio())
                {
                    opt.WithAudioCodec(AudioCodec.Aac);
                }
                //视频编码
                opt.WithVideoCodec(VideoCodec.LibX264);
                opt.ForceFormat("flv");
                opt.ForcePixelFormat("yuv420p");
                opt.WithConstantRateFactor(20);
                opt.UsingShortest();
            });
            return rt;
        }

        private void GetVideoInputArg()
        {
            this.FFMpegArguments = FFMpegArguments.FromPipeInput(this.PipeSource, opt =>
            {
                opt.ForceFormat("image2pipe");
                opt.WithFramerate(_frameRate);
                opt.Resize(this.DeviceProvider.Size.Width, this.DeviceProvider.Size.Height);

                WithMuteArgument(opt);
            });
        }

        public override void Dispose()
        {
            if (this.DeviceProvider != null)
                this.DeviceProvider.Dispose();
        }
    }
}
