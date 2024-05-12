using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Models.Settings;
using BilibiliAutoLiver.Services.FFMpeg.DeviceProviders;
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
            InputVideoSource sourceItem = new InputVideoSource()
            {
                Type = InputSourceType.Camera,
                Path = "HD Pro Webcam C920",
                Resolution = "1280*720",
                Index = 0,
            };

            CameraDeviceProvider deviceProvider = new CameraDeviceProvider(sourceItem, (p) =>
            {

            });

            await deviceProvider.Start();
        }
    }
}
