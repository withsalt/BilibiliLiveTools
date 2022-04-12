using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BilibiliLiverTests;
using BilibiliLiver.Services.Interface;

namespace BilibiliLiver.Services.Tests
{
    [TestClass()]
    public class BilibiliCookieServiceTests : BilibiliLiverTestsBase
    {
        private readonly IBilibiliCookieService _cookieService;

        public BilibiliCookieServiceTests()
        {
            _cookieService = (IBilibiliCookieService)ServiceProvider.GetService(typeof(IBilibiliCookieService));
            if (_cookieService == null)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void CookieDeserializeTest()
        {
            var values = _cookieService.CookieDeserialize(_cookieService.Get());
            Assert.IsNotNull(values);
        }

        [TestMethod()]
        public void GetTest()
        {
            string cookieText = _cookieService.Get();
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