using System.Threading.Tasks;
using BilibiliAutoLiver.Services.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BilibiliLiverTests.Services
{
    [TestClass()]
    public class PushStreamServiceTests : BilibiliLiverTestsBase
    {
        private readonly IPushStreamServiceV1 _pushStream;

        public PushStreamServiceTests()
        {
            _pushStream = (IPushStreamServiceV1)ServiceProvider.GetService(typeof(IPushStreamServiceV1));
            if (_pushStream == null)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public async Task FFmpegTestTest()
        {
            await _pushStream.CheckFFmpegBinary();
            Assert.Fail();
        }
    }
}