using BilibiliLiver.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BilibiliLiverTests;
using BilibiliLiver.Services.Interface;
using BilibiliLiveCommon.Services.Interface;
using BilibiliLiveCommon.Model;
using BilibiliLiveCommon.Utils;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;

namespace BilibiliLiver.Services.Tests
{
    [TestClass()]
    public class AccountServiceTests : BilibiliLiverTestsBase
    {
        private readonly IBilibiliAccountService _accountService;

        public AccountServiceTests()
        {
            _accountService = (IBilibiliAccountService)ServiceProvider.GetService(typeof(IBilibiliAccountService));
            if (_accountService == null)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public async Task LoginTest()
        {
            var result = await _accountService.LoginByCookie();
            if (result == null)
            {
                Assert.Fail();
                return;
            }
            Assert.IsTrue(result.IsLogin);
        }

        [TestMethod()]
        public async Task GenerateQrCodeTest()
        {
            var result = await _accountService.GenerateQrCode();
            if (result == null)
            {
                Assert.Fail();
                return;
            }
            Assert.IsNotNull(result.url);
        }

        [TestMethod()]
        public async Task QrCodeHasScanedTest()
        {
            QrCodeUrl qrCode = await _accountService.GenerateQrCode();

            using (MemoryStream stream = QrCodeGenerator.Generate(qrCode.url))
            {
                using (FileStream fileStream = new FileStream("qrcode.png", FileMode.Create))
                {
                    // 将内存流中的数据复制到文件流中
                    stream.Seek(0, SeekOrigin.Begin); // 将内存流的位置设置为起始位置
                    stream.CopyTo(fileStream);
                }
            }
            Stopwatch sw = Stopwatch.StartNew();
            while (true)
            {
                var result = await _accountService.QrCodeHasScaned(qrCode.qrcode_key);
                if (result.Data.status == QrCodeStatus.Scaned)
                {
                    Debug.WriteLine($"已扫描，结果：{JsonConvert.SerializeObject(result)}");
                    break;
                }
                if (result.Data.status == QrCodeStatus.Expired)
                {
                    Debug.WriteLine("已过期");
                    break;
                }
                Debug.WriteLine($"等待扫描中...耗时:{sw.ElapsedMilliseconds / 1000}s");
                await Task.Delay(1000);
            }
            sw.Stop();

        }

        [TestMethod()]
        public async Task HeartBeatTest()
        {
            await _accountService.HeartBeat();
        }
    }
}