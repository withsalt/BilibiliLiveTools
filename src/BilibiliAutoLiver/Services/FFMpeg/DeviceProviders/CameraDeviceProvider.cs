using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models;
using BilibiliAutoLiver.Models.Settings;
using FlashCap;
using SkiaSharp;

namespace BilibiliAutoLiver.Services.FFMpeg.DeviceProviders
{
    public class CameraDeviceProvider : ICameraDeviceProvider
    {
        public Size Size {  get; }

        private Action<BufferFrame> _onBuffer;
        private CaptureDevice _captureDevice;
        private CaptureDeviceDescriptor _captureDeviceDescriptor;
        private VideoCharacteristics _characteristics;
        private CancellationTokenSource _tokenSource;
        private int _countFrames;
        private static readonly object stopLocker = new object();
        private static bool _isStopped = false;

        public CameraDeviceProvider(InputVideoSource sourceItem, Action<BufferFrame> onBuffer)
        {
            if (onBuffer == null) throw new ArgumentException("On buffer action can not null");
            _onBuffer = onBuffer;

            List<CaptureDeviceDescriptor> devices = new CaptureDevices()?.EnumerateDescriptors().ToList();
            if (devices?.Any() != true)
            {
                throw new Exception($"找不到视频输入设备！");
            }
            if (!string.IsNullOrEmpty(sourceItem.Path))
            {
                _captureDeviceDescriptor = devices.Where(p => p.Name == sourceItem.Path).FirstOrDefault();
            }
            if (_captureDeviceDescriptor == null && sourceItem.Index >= 0 && sourceItem.Index < devices.Count)
            {
                _captureDeviceDescriptor = devices[sourceItem.Index];
            }
            if (_captureDeviceDescriptor == null && !string.IsNullOrEmpty(sourceItem.Path))
            {
                throw new Exception($"找不到名称为{sourceItem.Path}的视频输入设备！");
            }
            IEnumerable<VideoCharacteristics> targetCharacteristics = _captureDeviceDescriptor.Characteristics?.Where(p => p.Width == sourceItem.Width && p.Height == sourceItem.Height && p.PixelFormat != PixelFormats.Unknown);
            if (targetCharacteristics?.Any() != true)
            {
                throw new Exception($"视频输入设备{_captureDeviceDescriptor.Name}不支持分辨率{sourceItem.Resolution}");
            }
            _characteristics = targetCharacteristics.First();
            this.Size = new Size(_characteristics.Width, _characteristics.Height);
        }

        public async Task Start()
        {
            _tokenSource = new CancellationTokenSource();
            _captureDevice = await _captureDeviceDescriptor.OpenAsync(_characteristics, PixelBufferArrived).ConfigureAwait(false);
            if (_captureDevice == null)
            {
                throw new Exception($"无法打开视频输入设备设备：{_captureDeviceDescriptor.Name}");
            }
            await _captureDevice.StartAsync(_tokenSource.Token);
            _isStopped = false;
        }

        public Task Stop()
        {
            if (_isStopped)
                return Task.CompletedTask;
            lock (stopLocker)
            {
                if (_isStopped)
                    return Task.CompletedTask;
                if (_captureDevice.IsRunning)
                {
                    _tokenSource.Cancel();
                    Stopwatch sw = new Stopwatch();
                    while (_captureDevice.IsRunning && sw.ElapsedMilliseconds < 3000)
                    {
                        Thread.Sleep(0);
                    }
                }
                if (_captureDevice != null)
                {
                    _captureDevice.StopAsync().GetAwaiter().GetResult();
                    _captureDevice.Dispose();

                    if (_tokenSource != null)
                    {
                        _tokenSource.Dispose();
                        _tokenSource = null;
                    }
                    _captureDevice = null;
                }
                _isStopped = true;
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Stop().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private void PixelBufferArrived(PixelBufferScope bufferScope)
        {
            try
            {
                ArraySegment<byte> image = bufferScope.Buffer.ReferImage();
                if (image.Count <= 0 || image.Array == null)
                {
                    return;
                }
                int countFrames = Interlocked.Increment(ref _countFrames);
                long frameIndex = bufferScope.Buffer.FrameIndex;
                TimeSpan timestamp = bufferScope.Buffer.Timestamp;
                SKBitmap bitmap = SKBitmap.Decode(image);
                BufferFrame frame = new BufferFrame(bitmap, countFrames, frameIndex, timestamp);
                _onBuffer(frame);
            }
            finally
            {
                bufferScope.ReleaseNow();
            }
        }
    }
}
