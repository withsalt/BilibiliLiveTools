using System.Diagnostics;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace BilibiliLiverTests.Services
{
    [TestClass()]
    public class AccountServiceTests : BilibiliLiverTestsBase
    {
        private readonly IBilibiliAccountApiService _accountService;

        public AccountServiceTests()
        {
            _accountService = (IBilibiliAccountApiService)ServiceProvider.GetService(typeof(IBilibiliAccountApiService));
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

            byte[] qrCodeBytes = qrCode.GetBytes();
            Stopwatch sw = Stopwatch.StartNew();
            while (true)
            {
                var result = await _accountService.QrCodeScanStatus(qrCode.qrcode_key);
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