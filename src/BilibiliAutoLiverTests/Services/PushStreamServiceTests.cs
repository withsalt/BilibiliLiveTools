using BilibiliAutoLiver.Services.Interface;

namespace BilibiliLiverTests.Services
{
    [TestClass()]
    public class PushStreamServiceTests : BilibiliLiverTestsBase
    {
        private readonly IAdvancePushStreamService _pushStream;

        public PushStreamServiceTests()
        {
            _pushStream = (IAdvancePushStreamService)ServiceProvider.GetService(typeof(IAdvancePushStreamService));
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