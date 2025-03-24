using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Constants;
using Bilibili.AspNetCore.Apis.Exceptions;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models;
using Bilibili.AspNetCore.Apis.Models.Base;
using Bilibili.AspNetCore.Apis.Models.Enums;
using Microsoft.Extensions.Caching.Memory;
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
        /// 更新直播间公告
        /// </summary>
        private const string _updateRoomNews = "https://api.live.bilibili.com/xlive/app-blink/v1/index/updateRoomNews";

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
        private readonly IMemoryCache _cache;

        public BilibiliLiveApiService(ILogger<BilibiliLiveApiService> logger
            , IBilibiliCookieService cookie
            , IMemoryCache cache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cookie = cookie ?? throw new ArgumentNullException(nameof(cookie));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
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
            var data = await _cache.GetOrCreateAsync(CacheKeyConstant.ALL_LIVE_AREAS_CACHE_KEY, async p =>
            {
                p.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60);

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
            });
            return data;
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


        public async Task<bool> UpdateLiveRoomInfo(long roomId, string title, int areaId)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentNullException("title", "title不能为空");
            }
            if (title.Length > 20)
            {
                throw new ArgumentOutOfRangeException("title", "title不能超过20个字");
            }
            _ = await CheckArea(areaId);
            var postData = new
            {
                room_id = roomId,
                area_id = areaId,
                title = title,
                csrf_token = await _cookie.GetCsrf(),
                csrf = await _cookie.GetCsrf(),
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
                await Delay(nameof(UpdateLiveRoomInfo));
            }
        }

        public async Task<bool> UpdateRoomNews(long roomId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentNullException("content", "content不能为空");
            }
            if (content.Length > 60)
            {
                throw new ArgumentOutOfRangeException("content", "content不能超过60个字");
            }
            var postData = new
            {
                room_id = roomId,
                uid = await _cookie.GetUserId(),
                content = content,
                csrf_token = await _cookie.GetCsrf(),
                csrf = await _cookie.GetCsrf(),
            };
            try
            {
                var result = await _httpClient.Execute<ResultModel<object>>(_updateRoomNews, HttpMethod.Post, postData, BodyFormat.Form_UrlEncoded);
                if (result == null)
                {
                    throw new ApiRequestException(_updateRoomNews, HttpMethod.Post, "返回内容为空");
                }
                if (result.Code != 0)
                {
                    throw new ApiRequestException(_updateRoomNews, HttpMethod.Post, result.Message);
                }
                return result.Code == 0;
            }
            finally
            {
                await Delay(nameof(UpdateRoomNews));
            }
        }

        public async Task<StartLiveInfo> StartLive(long roomId, int areaId)
        {
            var areaItem = await CheckArea(areaId);
            var postData = new
            {
                room_id = roomId,
                platform = "android_link",
                area_v2 = areaItem.id,
                backup_stream = 0,
                csrf_token = await _cookie.GetCsrf(),
                csrf = await _cookie.GetCsrf(),
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
                csrf_token = await _cookie.GetCsrf(),
                csrf = await _cookie.GetCsrf(),
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
            int sleepMsec = new Random().Next(100, 1000);
            _logger.LogDebug($"执行{operationName}操作完成，休眠{sleepMsec}ms，避免被B站频繁操作。");
            await Task.Delay(sleepMsec);
        }

        #endregion
    }
}
