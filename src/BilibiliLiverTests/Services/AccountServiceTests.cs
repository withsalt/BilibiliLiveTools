using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using BilibiliLiverTests;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using BilibiliAutoLiver.Services.Interface;
using BilibiliAutoLiver.Model;
using BilibiliAutoLiver.Utils;

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
        public async Task CookieNeedToRefreshTest()
        {
            var result = await _accountService.CookieNeedToRefresh();
        }

        [TestMethod()]
        public async Task RefreshCookieTest()
        {
            UserInfo userInfo = await _accountService.LoginByCookie();

            await _accountService.RefreshCookie();

            userInfo = await _accountService.LoginByCookie();
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

            byte[] qrCodeBytes = QrCode.Generate(qrCode.url);
            {
                using (FileStream fileStream = new FileStream("qrcode.png", FileMode.Create))
                {
                    await fileStream.WriteAsync(qrCodeBytes, 0, qrCodeBytes.Length);
                }
            }
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