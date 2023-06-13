using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BilibiliLiveCommon.Services.Interface;
using BilibiliLiverTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var result = await _accountService.Login();
            if (result == null)
            {
                Assert.Fail();
                return;
            }
            Assert.IsTrue(result.IsLogin);
        }

        [TestMethod()]
        public async Task HeartBeatTest()
        {
            await _accountService.HeartBeat();
        }

        [TestMethod()]
        public async Task RefeshTest()
        {
            await _accountService.Refesh();
        }

        /// <summary>
        /// {
        ///     "timestamp": "1123",
        ///     "hash": "6ff84c56638ab54b0dc27a6bcff6737e4a8d1accc36ffd7f87dc8efbf0b0aab5a21ba27deb6a8bf6f28362883ff5d945ec7b3539c0a46dc43854c9e6197c5cc035e17bd256245662ff195080d6321ed176203109ed920b378604c8d4a59e0bcc27c2b37499fe7a0e9a1beea2bf8397cebbea99e8ab60bdf2a5835d4de4317939",
        ///     "code": 0
        /// }
        /// </summary>
        [TestMethod()]
        public void TestRSA_OAEP()
        {
            string timestamp = "refresh_1123";
            string encode = GetCorrespondPath(timestamp);
            bool isEqual = encode.Equals("6ff84c56638ab54b0dc27a6bcff6737e4a8d1accc36ffd7f87dc8efbf0b0aab5a21ba27deb6a8bf6f28362883ff5d945ec7b3539c0a46dc43854c9e6197c5cc035e17bd256245662ff195080d6321ed176203109ed920b378604c8d4a59e0bcc27c2b37499fe7a0e9a1beea2bf8397cebbea99e8ab60bdf2a5835d4de4317939");
        }

        static readonly string publickey = @"-----BEGIN PUBLIC KEY----- MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDLgd2OAkcGVtoE3ThUREbio0Eg Uc/prcajMKXvkCKFCWhJYJcLkcM2DKKcSeFpD/j6Boy538YXnR6VhcuUJOhH2x71 nzPjfdTcqMz7djHum0qSZA0AyCBDABUqCrfNgCiJ00Ra7GmRj+YCK1NJEuewlb40 JNrRuoEUXpabUzGB8QIDAQAB -----END PUBLIC KEY-----";


        public static string GetCorrespondPath(string timestamp)
        {
            try
            {
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportFromPem(publickey);
                    byte[] data = Encoding.UTF8.GetBytes(timestamp);
                    byte[] encryptedData = rsa.Encrypt(data, true);
                    return BitConverter.ToString(encryptedData).Replace("-", "").ToLower();
                }
            }
            catch( Exception ex)
            {
                return null;
            }
        }
    }
}