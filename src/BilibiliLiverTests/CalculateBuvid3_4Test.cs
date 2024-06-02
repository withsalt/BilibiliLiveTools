using System.Security.Cryptography;
using Bilibili.AspNetCore.Apis.Services.Cookie;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BilibiliLiverTests
{
    [TestClass()]
    public class CalculateBuvid3_4Test
    {
        [TestMethod()]
        public void Test()
        {
            string uuid = "876CB7109F-98DE-BDBE-8E108-510E1044B4D4239710infoc";
            ulong ts = 1717144979;
            //var rt = new Buvid3_4Calculator(uuid, ts).Generate();
            //Assert.AreEqual(rt, "6f17099fc63029e5c107bdde792d987");

        }
    }
}
