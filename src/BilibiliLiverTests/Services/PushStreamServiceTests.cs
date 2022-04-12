using Microsoft.VisualStudio.TestTools.UnitTesting;
using BilibiliLiver.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BilibiliLiver.Services.Interface;
using BilibiliLiverTests;
using BilibiliLiver.Utils;

namespace BilibiliLiver.Services.Tests
{
    [TestClass()]
    public class PushStreamServiceTests : BilibiliLiverTestsBase
    {
        private readonly IPushStreamService _pushStream;

        public PushStreamServiceTests()
        {
            _pushStream = (IPushStreamService)ServiceProvider.GetService(typeof(IPushStreamService));
            if (_pushStream == null)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public async Task FFmpegTestTest()
        {
            bool result = await _pushStream.FFmpegTest();
            Assert.Fail();
        }

        [TestMethod()]
        public async Task NetworkCheckTest()
        {
            bool result = await NetworkUtil.Ping();
            Assert.IsTrue(result);
        }
    }
}