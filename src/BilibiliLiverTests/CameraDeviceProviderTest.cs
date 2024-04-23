using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Services.DeviceProviders;
using FlashCap;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BilibiliLiverTests
{
    [TestClass()]
    public class CameraDeviceProviderTest
    {
        [TestMethod()]
        public async Task FlashCapCameraDeviceProviderTest()
        {
            InputVideoSourceItem sourceItem = new InputVideoSourceItem()
            {
                Type = InputVideoSourceType.Device,
                Path = "HD Pro Webcam C9201",
                Resolution = "1280*720",
                Index = 0,
            };

            FlashCapCameraDeviceProvider deviceProvider = new FlashCapCameraDeviceProvider(sourceItem, OnPixelBufferArrived);

            await deviceProvider.Start();
        }

        private static void OnPixelBufferArrived(PixelBufferScope bufferScope)
        {
            try
            {
                //if (que.Count > _frameRate * 2)
                //{
                //    Console.WriteLine("帧队列堆积，丢弃...");
                //    que.Clear();
                //}
                //if (sw.ElapsedMilliseconds - last > 1000)
                //{
                //    Console.WriteLine($"帧队列数量：{que.Count}");
                //    last = sw.ElapsedMilliseconds;
                //}
                //ArraySegment<byte> image = bufferScope.Buffer.ReferImage();
                //if (image.Count > 0 && image.Array != null)
                //{
                //    que.Enqueue(image.Array);
                //}



                // Capture statistics variables.
                //var countFrames = Interlocked.Increment(ref _countFrames);
                //var frameIndex = bufferScope.Buffer.FrameIndex;
                //var timestamp = bufferScope.Buffer.Timestamp;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message, ex);
            }
            finally
            {
                bufferScope.ReleaseNow();
            }
        }
    }
}
