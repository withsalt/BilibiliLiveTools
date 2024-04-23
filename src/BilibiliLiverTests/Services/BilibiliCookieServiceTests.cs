using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models;
using Bilibili.AspNetCore.Apis.Models.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BilibiliLiverTests.Services
{
    [TestClass()]
    public class BilibiliCookieServiceTests : BilibiliLiverTestsBase
    {
        private readonly IBilibiliCookieService _cookieService;
        private readonly IBilibiliAccountApiService _accountService;

        public BilibiliCookieServiceTests()
        {
            _cookieService = (IBilibiliCookieService)ServiceProvider.GetService(typeof(IBilibiliCookieService));
            if (_cookieService == null)
            {
                Assert.Fail();
            }
            _accountService = (IBilibiliAccountApiService)ServiceProvider.GetService(typeof(IBilibiliAccountApiService));
            if (_accountService == null)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public async Task SaveTest()
        {
            var scanResult = await GetQrCodeResult();
            await _cookieService.SaveCookie(scanResult.Cookies, scanResult.Data.refresh_token);
        }

        [TestMethod()]
        public Task AllTest()
        {
            var cookies = _cookieService.GetCookies();
            var cookieString = _cookieService.GetString();

            bool hasCookie = _cookieService.HasCookie();
            Assert.IsTrue(hasCookie);



            cookies = _cookieService.GetCookies(true);
            cookieString = _cookieService.GetString(true);

            var csrf = _cookieService.GetCsrf();
            var userId = _cookieService.GetUserId();

            string token = _cookieService.GetRefreshToken();
            Assert.IsNotNull(token);

            return Task.CompletedTask;
        }

        public async Task<ResultModel<QrCodeScanResult>> GetQrCodeResult()
        {
            QrCodeUrl qrCode = await _accountService.GenerateQrCode();

            byte[] qrCodeBytes = qrCode.GetBytes();
            Stopwatch sw = Stopwatch.StartNew();
            while (true)
            {
                var result = await _accountService.QrCodeScanStatus(qrCode.qrcode_key);
                if (result.Data.status == QrCodeStatus.Scaned)
                {
                    return result;
                }
                if (result.Data.status == QrCodeStatus.Expired)
                {
                    throw new Exception("二维码已过期");
                }
                Debug.WriteLine($"等待扫描中...耗时:{sw.ElapsedMilliseconds / 1000}s");
                await Task.Delay(1000);
            }
        }



        [TestMethod()]
        public void GetTest()
        {
            string cookieText = _cookieService.GetString();
            Assert.IsTrue(!string.IsNullOrEmpty(cookieText));
        }

        [TestMethod()]
        public void GetCsrfTest()
        {
            string csrf = _cookieService.GetCsrf();
            Assert.IsNotNull(csrf);
        }

        [TestMethod()]
        public void GetUserIdTest()
        {
            string userid = _cookieService.GetUserId();
            Assert.IsNotNull(userid);
        }
    }
}