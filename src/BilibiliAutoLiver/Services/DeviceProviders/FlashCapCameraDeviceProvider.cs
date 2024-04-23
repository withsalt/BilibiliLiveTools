using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models;
using FlashCap;

namespace BilibiliAutoLiver.Services.DeviceProviders
{
    public class FlashCapCameraDeviceProvider : ICameraDeviceProvider
    {
        private Action<byte[]> _onBuffer;

        private CaptureDevice _captureDevice;

        public FlashCapCameraDeviceProvider(InputVideoSourceItem sourceItem, PixelBufferArrivedDelegate onBuffer)
        {
            if (onBuffer == null) throw new ArgumentException("On buffer action can not null");
            _onBuffer = onBuffer;

            List<CaptureDeviceDescriptor> devices = new CaptureDevices()?.EnumerateDescriptors().ToList();
            if (devices?.Any() != true)
            {
                throw new Exception($"找不到视频输入设备！");
            }
            CaptureDeviceDescriptor device = null;
            if (!string.IsNullOrEmpty(sourceItem.Path))
            {
                device = devices.Where(p => p.Name == sourceItem.Path).FirstOrDefault();
            }
            if (device == null && sourceItem.Index >= 0 && sourceItem.Index < devices.Count)
            {
                device = devices[sourceItem.Index];
            }
            if (device == null && !string.IsNullOrEmpty(sourceItem.Path))
            {
                throw new Exception($"找不到名称为{sourceItem.Path}的视频输入设备！");
            }
            IEnumerable<VideoCharacteristics> targetCharacteristics = device.Characteristics?.Where(p => p.Width == sourceItem.Width && p.Height == sourceItem.Height && p.PixelFormat != PixelFormats.Unknown);
            if (targetCharacteristics?.Any() != true)
            {
                throw new Exception($"视频输入设备{device.Name}不支持分辨率{sourceItem.Resolution}");
            }
            VideoCharacteristics characteristics = targetCharacteristics.First();
            _captureDevice = device.OpenAsync(characteristics, _onBuffer).GetAwaiter().GetResult();
            if (_captureDevice == null)
            {
                throw new Exception($"无法打开视频输入设备设备：{device.Name}");
            }
        }

        public async Task Start()
        {
            await _captureDevice.StartAsync();
        }

        public Task Stop()
        {
            return Task.CompletedTask;
        }

        private void PixelBufferArrived(PixelBufferScope bufferScope)
        {

        }
    }

    public class BufferFrame
    {
        public byte[] Bytes { get; set; }

        public int FrameCount { get; set; }

        public int FrameIndex { get; set; }

        public int Timestamp { get; set; }
    }
}
