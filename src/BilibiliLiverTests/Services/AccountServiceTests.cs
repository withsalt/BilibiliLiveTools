using BilibiliLiver.Services;
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
    public class AccountServiceTests : BilibiliLiverTestsBase
    {
        private readonly IAccountService _accountService;

        public AccountServiceTests()
        {
            _accountService = (IAccountService)ServiceProvider.GetService(typeof(IAccountService));
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
    }
}