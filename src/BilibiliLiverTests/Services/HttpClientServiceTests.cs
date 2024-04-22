using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using BilibiliLiverTests;
using System.Net.Http;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models.Enums;

namespace BilibiliLiverTests.Services
{
    [TestClass()]
    public class HttpClientServiceTests : BilibiliLiverTestsBase
    {
        private readonly IHttpClientService _apiService;

        public HttpClientServiceTests()
        {
            _apiService = (IHttpClientService)ServiceProvider.GetService(typeof(IHttpClientService));
            if (_apiService == null)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public async Task ExecuteTest()
        {
            string url = "https://localhost:7174/WeatherForecast";

            var result = await _apiService.Execute<TestData>(url, HttpMethod.Post, "{\"Id\":\"1\",\"Name\":\"111\"}", BodyFormat.Json);

            Assert.IsNotNull(result);
        }
    }

    class TestData
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }
}