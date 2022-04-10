using BilibiliLiver.Model;
using Microsoft.Extensions.Logging;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLiver.Services
{
    public class BilibiliLiveApiService : IBilibiliLiveApiService
    {
        /// <summary>
        /// 获取直播间信息
        /// </summary>
        private const string _getLiveRoomInfoApi = "https://api.live.bilibili.com/xlive/app-blink/v1/room/GetInfo?platform=pc";

        /// <summary>
        /// 更新直播间名称
        /// </summary>
        private const string _updateLiveRoomName = "https://api.live.bilibili.com/room/v1/Room/update";

        private readonly ILogger<BilibiliLiveApiService> _logger;
        private readonly IHttpClientService _httpClient;
        private readonly IBilibiliCookieService _cookie;

        public BilibiliLiveApiService(ILogger<BilibiliLiveApiService> logger
            , IHttpClientService httpClient
            , IBilibiliCookieService cookie)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _cookie = cookie ?? throw new ArgumentNullException(nameof(cookie));
        }

        public async Task<LiveRoomInfo> GetLiveRoomInfo()
        {
            var liveRoomInfo = await _httpClient.Execute<ResultModel<LiveRoomInfo>>(_getLiveRoomInfoApi, Method.Get);
            if (liveRoomInfo == null)
            {
                throw new ApiRequestException(_getLiveRoomInfoApi, Method.Get, "返回内容为空");
            }
            return liveRoomInfo.Data;
        }

        public async Task<bool> UpdateLiveRoomName(int roomId, string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentNullException(nameof(title), "直播间名称不能为空！");
            }
            var postData = new
            {
                room_id = roomId,
                title = title,
                csrf_token = _cookie.GetCsrf(),
                csrf = _cookie.GetCsrf(),
            };
            var result = await _httpClient.Execute<ResultModel<Array>>(_updateLiveRoomName, Method.Post, postData);
            if (result == null)
            {
                throw new ApiRequestException(_getLiveRoomInfoApi, Method.Get, "返回内容为空");
            }
            if (result.Code != 0)
            {
                throw new ApiRequestException(_getLiveRoomInfoApi, Method.Get, result.Message);
            }
            return result.Code == 0;
        }

    }
}
