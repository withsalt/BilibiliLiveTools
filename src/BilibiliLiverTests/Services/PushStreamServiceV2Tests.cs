using System.Threading.Tasks;
using BilibiliAutoLiver.Services.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BilibiliLiverTests.Services
{
    [TestClass()]
    public class PushStreamServiceV2Tests : BilibiliLiverTestsBase
    {
        private readonly IPushStreamServiceV2 _pushStream;

        public PushStreamServiceV2Tests()
        {
            _pushStream = (IPushStreamServiceV2)ServiceProvider.GetService(typeof(IPushStreamServiceV2));
            if (_pushStream == null)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public async Task FFmpegTestTest()
        {
            await _pushStream.Start();
            Assert.Fail();
        }
    }
}