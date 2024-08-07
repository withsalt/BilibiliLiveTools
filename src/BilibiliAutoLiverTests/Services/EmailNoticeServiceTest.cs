using System.Threading.Tasks;
using BilibiliAutoLiver.Services.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BilibiliLiverTests.Services
{
    [TestClass()]
    public class EmailNoticeServiceTest : BilibiliLiverTestsBase
    {
        private readonly IEmailNoticeService _emailNoticeService;

        public EmailNoticeServiceTest()
        {
            _emailNoticeService = (IEmailNoticeService)ServiceProvider.GetService(typeof(IEmailNoticeService));
            if (_emailNoticeService == null)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public async Task SendTest()
        {
            var rt = await _emailNoticeService.Send("发件测试", "啦啦啦啦啦");
        }
    }
}
