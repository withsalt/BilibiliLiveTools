using Microsoft.VisualStudio.TestTools.UnitTesting;
using BilibiliLiver.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BilibiliLiverTests;
using BilibiliLiver.Services.Interface;
using System.Net.Http;

namespace BilibiliLiver.Services.Tests
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

            var result = await _apiService.Execute<TestData>(url, HttpMethod.Post, "{\"Id\":\"1\",\"Name\":\"111\"}", Model.Enums.BodyFormat.Json);

            Assert.IsNotNull(result);
        }
    }

    class TestData
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }
}