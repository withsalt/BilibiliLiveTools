using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Exceptions;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models;
using Bilibili.AspNetCore.Apis.Models.Base;
using Bilibili.AspNetCore.Apis.Models.Enums;
using Microsoft.Extensions.Logging;

namespace Bilibili.AspNetCore.Apis.Services
{
    public class BilibiliLiveApiService : IBilibiliLiveApiService
    {
        /// <summary>
        /// 获取直播间信息
        /// </summary>
        private const string _getMyLiveRoomInfoApi = "https://api.live.bilibili.com/xlive/app-blink/v1/room/GetInfo?platform=pc";

        /// <summary>
        /// 根据id获取直播间信息
        /// </summary>
        private const string _getLiveRoomInfoByIdApi = "https://api.live.bilibili.com/room/v1/Room/get_info?room_id={0}";

        /// <summary>
        /// 更新直播间名称
        /// </summary>
        private const string _updateLiveRoomNameApi = "https://api.live.bilibili.com/room/v1/Room/update";

        /// <summary>
        /// 获取直播种类
        /// </summary>
        private const string _getLiveCategoryApi = "https://api.live.bilibili.com/room/v1/Area/getList";

        /// <summary>
        /// 开启直播
        /// </summary>
        private const string _startLiveApi = "https://api.live.bilibili.com/room/v1/Room/startLive";

        /// <summary>
        /// 停止直播
        /// </summary>
        private const string _stopLiveApi = "https://api.live.bilibili.com/room/v1/Room/stopLive";

        /// <summary>
        /// 获取直播间直播信息
        /// </summary>
        private const string _getRoomPlayInfo = "https://api.live.bilibili.com/xlive/web-room/v2/index/getRoomPlayInfo?room_id={0}&protocol=0,1&format=0,1,2&codec=0,1&qn=0&platform=web&ptype=8&dolby=5";


        private readonly ILogger<BilibiliLiveApiService> _logger;
        private readonly IHttpClientService _httpClient;
        private readonly IBilibiliCookieService _cookie;

        public BilibiliLiveApiService(ILogger<BilibiliLiveApiService> logger
            , IBilibiliCookieService cookie)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cookie = cookie ?? throw new ArgumentNullException(nameof(cookie));
            _httpClient = new HttpClientService(_cookie);
        }

        public async Task<MyLiveRoomInfo> GetMyLiveRoomInfo()
        {
            var result = await _httpClient.Execute<MyLiveRoomInfo>(_getMyLiveRoomInfoApi, HttpMethod.Get);
            if (result == null)
            {
                throw new ApiRequestException(_getMyLiveRoomInfoApi, HttpMethod.Get, "返回内容为空");
            }
            if (result.Code != 0)
            {
                throw new ApiRequestException(_getMyLiveRoomInfoApi, HttpMethod.Get, result.Message);
            }
            return result.Data;
        }

        public async Task<LiveRoomInfo> GetLiveRoomInfo(long roomId)
        {
            var result = await _httpClient.Execute<LiveRoomInfo>(string.Format(_getLiveRoomInfoByIdApi, roomId), HttpMethod.Get, withCookie: false);
            if (result == null)
            {
                throw new ApiRequestException(_getLiveRoomInfoByIdApi, HttpMethod.Get, "返回内容为空");
            }
            if (result.Code != 0)
            {
                throw new ApiRequestException(_getLiveRoomInfoByIdApi, HttpMethod.Get, result.Message);
            }
            return result.Data;
        }

        public async Task<List<LiveAreaItem>> GetLiveAreas()
        {
            var result = await _httpClient.Execute<List<LiveAreaItem>>(_getLiveCategoryApi, HttpMethod.Get, null, BodyFormat.Json, false);
            if (result == null)
            {
                throw new ApiRequestException(_getLiveCategoryApi, HttpMethod.Get, "返回内容为空");
            }
            if (result.Code != 0)
            {
                throw new ApiRequestException(_getLiveCategoryApi, HttpMethod.Get, result.Message);
            }
            return result.Data;
        }

        public async Task<RoomPlayInfo> GetRoomPlayInfo(long roomId)
        {
            if (roomId <= 0)
            {
                throw new ArgumentNullException(nameof(roomId));
            }
            var result = await _httpClient.Execute<RoomPlayInfo>(string.Format(_getRoomPlayInfo, roomId), HttpMethod.Get, null, BodyFormat.Json, false);
            if (result == null)
            {
                throw new ApiRequestException(string.Format(_getRoomPlayInfo, roomId), HttpMethod.Get, "返回内容为空");
            }
            if (result.Code != 0)
            {
                throw new ApiRequestException(string.Format(_getRoomPlayInfo, roomId), HttpMethod.Get, result.Message);
            }
            return result.Data;
        }

        public async Task<bool> UpdateLiveRoomName(long roomId, string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentNullException(nameof(title), "直播间名称不能为空！");
            }
            var postData = new
            {
                room_id = roomId,
                title,
                csrf_token = _cookie.GetCsrf(),
                csrf = _cookie.GetCsrf(),
            };
            try
            {
                var result = await _httpClient.Execute<object>(_updateLiveRoomNameApi, HttpMethod.Post, postData, BodyFormat.Form_UrlEncoded);
                if (result == null)
                {
                    throw new ApiRequestException(_updateLiveRoomNameApi, HttpMethod.Post, "返回内容为空");
                }
                if (result.Code != 0)
                {
                    throw new ApiRequestException(_updateLiveRoomNameApi, HttpMethod.Post, result.Message);
                }
                return result.Code == 0;
            }
            finally
            {
                await Delay(nameof(UpdateLiveRoomName));
            }
        }

        public async Task<bool> UpdateLiveRoomArea(long roomId, int areaId)
        {
            _ = await CheckArea(areaId);
            var postData = new
            {
                room_id = roomId,
                area_id = areaId,
                csrf_token = _cookie.GetCsrf(),
                csrf = _cookie.GetCsrf(),
            };
            try
            {
                var result = await _httpClient.Execute<ResultModel<object>>(_updateLiveRoomNameApi, HttpMethod.Post, postData, BodyFormat.Form_UrlEncoded);
                if (result == null)
                {
                    throw new ApiRequestException(_updateLiveRoomNameApi, HttpMethod.Post, "返回内容为空");
                }
                if (result.Code != 0)
                {
                    throw new ApiRequestException(_updateLiveRoomNameApi, HttpMethod.Post, result.Message);
                }
                return result.Code == 0;
            }
            finally
            {
                await Delay(nameof(UpdateLiveRoomArea));
            }
        }

        public async Task<StartLiveInfo> StartLive(long roomId, int areaId)
        {
            var areaItem = await CheckArea(areaId);
            var postData = new
            {
                room_id = roomId,
                platform = "pc",
                area_v2 = areaItem.id,
                csrf_token = _cookie.GetCsrf(),
                csrf = _cookie.GetCsrf(),
            };
            try
            {
                var result = await _httpClient.Execute<StartLiveInfo>(_startLiveApi, HttpMethod.Post, postData, BodyFormat.Form_UrlEncoded);
                if (result == null)
                {
                    throw new ApiRequestException(_startLiveApi, HttpMethod.Post, "返回内容为空");
                }
                if (result.Code != 0)
                {
                    throw new ApiRequestException(_startLiveApi, HttpMethod.Post, result.Message);
                }
                if (result.Data.need_face_auth)
                {
                    throw new Exception("开启直播失败，需要进行人脸识别！");
                }
                return result.Data;
            }
            finally
            {
                await Delay(nameof(StartLive));
            }
        }

        public async Task<StopLiveInfo> StopLive(long roomId)
        {
            var postData = new
            {
                room_id = roomId,
                platform = "pc",
                csrf_token = _cookie.GetCsrf(),
                csrf = _cookie.GetCsrf(),
            };
            try
            {
                var result = await _httpClient.Execute<StopLiveInfo>(_stopLiveApi, HttpMethod.Post, postData, BodyFormat.Form_UrlEncoded);
                if (result == null)
                {
                    throw new ApiRequestException(_stopLiveApi, HttpMethod.Post, "返回内容为空");
                }
                if (result.Code != 0)
                {
                    throw new ApiRequestException(_stopLiveApi, HttpMethod.Post, result.Message);
                }
                return result.Data;
            }
            finally
            {
                await Delay(nameof(StopLive));
            }
        }

        #region private

        /// <summary>
        /// 根据Id获取分区
        /// </summary>
        /// <param name="source"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private LiveAreaItem FindItemFromLiveAreaTree(List<LiveAreaItem> source, int id)
        {
            foreach (var item in source)
            {
                if (item.id == id)
                {
                    return item;
                }
                if (item.list != null && item.list.Any())
                {
                    var result = FindItemFromLiveAreaTree(item.list, id);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 验证直播分区是否正确
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        private async Task<LiveAreaItem> CheckArea(int areaId)
        {
            if (areaId <= 0)
            {
                throw new ArgumentNullException(nameof(areaId), "直播间分类不能为空！");
            }
            List<LiveAreaItem> liveAreas = await GetLiveAreas();
            if (liveAreas == null || !liveAreas.Any())
            {
                throw new Exception("获取直播间分区信息失败！");
            }
            LiveAreaItem areaItem = FindItemFromLiveAreaTree(liveAreas, areaId);
            if (areaItem == null)
            {
                throw new Exception($"根据Id[{areaId}]获取直播间分区信息失败！");
            }
            return areaItem;
        }

        private async Task Delay(string operationName)
        {
            int sleepMsec = new Random().Next(100, 500);
            _logger.LogDebug($"执行{operationName}操作完成，休眠{sleepMsec}ms，避免被B站频繁操作。");
            await Task.Delay(sleepMsec);
        }

        #endregion
    }
}
