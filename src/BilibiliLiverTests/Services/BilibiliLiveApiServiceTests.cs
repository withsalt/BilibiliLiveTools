using Microsoft.VisualStudio.TestTools.UnitTesting;
using BilibiliLiver.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BilibiliLiverTests;

namespace BilibiliLiver.Services.Tests
{
    [TestClass()]
    public class BilibiliLiveApiServiceTests : BilibiliLiverTestsBase
    {
        private readonly IBilibiliLiveApiService _apiService;

        public BilibiliLiveApiServiceTests()
        {
            _apiService = (IBilibiliLiveApiService)ServiceProvider.GetService(typeof(IBilibiliLiveApiService));
            if (_apiService == null)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public async Task GetLiveRoomInfoTest()
        {
            var info = await _apiService.GetLiveRoomInfo();
            Assert.IsNotNull(info);
        }

        [TestMethod()]
        public async Task UpdateLiveRoomNameTest()
        {
            var info = await _apiService.GetLiveRoomInfo();
            bool reslt = await _apiService.UpdateLiveRoomName(info.room_id, info.title);
            Assert.Fail();
        }
    }
}