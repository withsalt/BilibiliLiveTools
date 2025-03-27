using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Utils;
using FlashCap;
using SkiaSharp;

namespace BilibiliAutoLiver.Services.FFMpeg.DeviceProviders
{
    public class CameraDeviceProvider : ICameraDeviceProvider
    {
        public Size Size { get; }

        private Action<SKBitmap> _onBuffer;
        private CaptureDevice _captureDevice;
        private CaptureDeviceDescriptor _captureDeviceDescriptor;
        private VideoCharacteristics _characteristics;
        private CancellationTokenSource _tokenSource;
        private static readonly object stopLocker = new object();
        private bool _isStopped = false;

        public CameraDeviceProvider(PushSettingDto pushSetting, Action<SKBitmap> onBuffer)
        {
            if (onBuffer == null) throw new ArgumentException("On buffer action can not null");
            _onBuffer = onBuffer;

            List<CaptureDeviceDescriptor> devices = new CaptureDevices()?.EnumerateDescriptors().ToList();
            if (devices?.Any() != true)
            {
                throw new Exception($"找不到视频输入设备！");
            }
            (string format, string deviceName) = CommonHelper.GetDeviceFormatAndName(pushSetting.DeviceName);
            if (!string.IsNullOrEmpty(deviceName))
            {
                _captureDeviceDescriptor = devices.FirstOrDefault(p => (p.Name == deviceName || p.Identity.ToString() == deviceName) && p.Characteristics?.Any() == true);
            }
            if (int.TryParse(deviceName, out int index)
                && index >= 0
                && _captureDeviceDescriptor == null
                && index < devices.Count)
            {
                _captureDeviceDescriptor = devices[index];
            }
            if (_captureDeviceDescriptor == null && !string.IsNullOrEmpty(deviceName))
            {
                throw new Exception($"找不到名称为{deviceName}的视频输入设备！");
            }
            if (pushSetting.InputWidth == 0 || pushSetting.InputHeight == 0)
            {
                throw new Exception($"当视频输入类型为设备时，分辨率不能为空！");
            }
            IEnumerable<VideoCharacteristics> targetCharacteristics = _captureDeviceDescriptor.Characteristics?.Where(p => p.Width == pushSetting.InputWidth && p.Height == pushSetting.InputHeight && p.PixelFormat != PixelFormats.Unknown);
            if (targetCharacteristics?.Any() != true)
            {
                throw new Exception($"视频输入设备{_captureDeviceDescriptor.Name}不支持分辨率{pushSetting.InputWidth}x{pushSetting.InputHeight}");
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
                try
                {
                    if (_isStopped)
                        return Task.CompletedTask;
                    if (_captureDevice == null)
                    {
                        if (_tokenSource != null)
                        {
                            _tokenSource.Cancel();
                            _tokenSource.Dispose();
                            _tokenSource = null;
                        }
                        return Task.CompletedTask;
                    }

                    _tokenSource.Cancel();
                    _captureDevice.StopAsync().GetAwaiter().GetResult();
                    if (_captureDevice.IsRunning)
                    {
                        Stopwatch sw = Stopwatch.StartNew();
                        while (_captureDevice.IsRunning && sw.ElapsedMilliseconds < 3000)
                        {
                            Thread.Sleep(0);
                        }
                        sw.Stop();
                    }
                    _captureDevice.Dispose();
                }
                finally
                {
                    if (_tokenSource != null)
                    {
                        _tokenSource.Dispose();
                        _tokenSource = null;
                    }
                    _captureDevice = null;

                    _isStopped = true;
                }
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
                SKBitmap bitmap = SKBitmap.Decode(image);
                if (bitmap == null)
                {
                    return;
                }
                _onBuffer(bitmap);
            }
            finally
            {
                bufferScope.ReleaseNow();
            }
        }
    }
}
