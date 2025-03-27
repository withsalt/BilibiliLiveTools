using BilibiliAutoLiver.Services.Interface;

namespace BilibiliLiverTests.Services
{
    [TestClass()]
    public class PushStreamServiceV2Tests : BilibiliLiverTestsBase
    {
        private readonly INormalPushStreamService _pushStream;

        public PushStreamServiceV2Tests()
        {
            _pushStream = (INormalPushStreamService)ServiceProvider.GetService(typeof(INormalPushStreamService));
            if (_pushStream == null)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public async Task FFmpegTestTest()
        {
            await _pushStream.Start(false);
            Assert.Fail();
        }
    }
}