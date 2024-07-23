using System.Threading.Tasks;
using BilibiliAutoLiver.Services.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BilibiliLiverTests.Services
{
    [TestClass()]
    public class FFMpegServiceTests : BilibiliLiverTestsBase
    {
        private readonly IFFMpegService _ffmpeg;

        public FFMpegServiceTests()
        {
            _ffmpeg = (IFFMpegService)ServiceProvider.GetService(typeof(IFFMpegService));
            if (_ffmpeg == null)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void FFmpegTest()
        {
            _ffmpeg.GetBinaryPath();
        }

        [TestMethod()]
        public async Task GetVersionTest()
        {
            var verviosn = await _ffmpeg.GetVersion();
        }

        [TestMethod()]
        public async Task GetVideoDevicesTest()
        {
             await _ffmpeg.GetVideoDevices();
        }

        [TestMethod()]
        public async Task GetAudioDevices()
        {
            await _ffmpeg.GetAudioDevices();
        }
    }
}