using System;
using System.Extensions;
using System.Net.Http;
using System.Threading.Tasks;
using Bilibili.Helper;
using Bilibili.Model;
using Bilibili.Model.Live;
using Bilibili.Model.Live.LiveRoomInfo;
using Bilibili.Model.Live.LiveRoomStreamInfo;
using Bilibili.Model.Live.StartLiveInfo;
using Bilibili.System.Extensions;
using Newtonsoft.Json.Linq;

namespace Bilibili.Api
{
    /// <summary>
    /// 直播API
    /// </summary>
    public class LiveApi
    {
        private const string DynRoomsUrl = "http://room.lc4t.cn:8000/dyn_rooms/";

        /// <summary>
        /// 获取直播房间号
        /// </summary>
        private const string GetRoomIdApi = "https://api.live.bilibili.com/live_user/v1/UserInfo/live_info";

        /// <summary>
        /// 获取直播信息接口
        /// </summary>
        private const string GetStreamByRoomIdApi = "https://api.live.bilibili.com/live_stream/v1/StreamList/get_stream_by_roomId";

        /// <summary>
        /// 开始直播的API
        /// </summary>
        private const string StartLiveApi = "https://api.live.bilibili.com/room/v1/Room/startLive";

        /// <summary>
        /// 动态获取房间ID列表
        /// </summary>
        /// <param name="start">起始序号（闭区间）</param>
        /// <param name="end">终止序号（开区间）</param>
        /// <returns></returns>
        public async Task<uint[]> GetRoomIdsDynamicAsync(uint start, uint end)
        {
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.SendAsync(HttpMethod.Get, DynRoomsUrl + start.ToString() + "-" + end.ToString()))
                return JObject.Parse(await response.Content.ReadAsStringAsync())["roomid"].ToObject<uint[]>();
        }

        /// <summary>
        /// 获取直播房间号
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<string> GetRoomId(User user)
        {
            if (user == null || !user.IsLogin)
            {
                throw new Exception("User unlogin.");
            }
            try
            {
                using (HttpResponseMessage response = await user.Client.SendAsync(HttpMethod.Get, GetRoomIdApi, null, user.AppHeaders, user.Data.Cookie))
                {
                    ResultModel<LiveRoomDataInfo> resultModel = await response.ConvertResultModel<LiveRoomDataInfo>();
                    if (resultModel.Code == 0 && !string.IsNullOrEmpty(resultModel.Data.RoomId))
                    {
                        return resultModel.Data.RoomId;
                    }
                    else
                    {
                        throw new Exception($"Get user live stram info failed. error code is {resultModel.Code}({resultModel.Msg}).");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取房间信息
        /// 暂时没用上
        /// </summary>
        /// <returns></returns>
        private async Task<LiveRoomStreamDataInfo> GetLiveRoomInfo(User user, string roomId)
        {
            if (user == null || !user.IsLogin)
            {
                throw new Exception("User unlogin.");
            }
            if (string.IsNullOrEmpty(roomId))
            {
                throw new Exception("Room id cannot null.");
            }
            try
            {
                var queries = new QueryCollection {
                    { "room_id", roomId }
                };
                using (HttpResponseMessage response = await user.Client.SendAsync(HttpMethod.Get, GetStreamByRoomIdApi, queries, user.AppHeaders, user.Data.Cookie))
                {
                    ResultModel<LiveRoomStreamDataInfo> resultModel = await response.ConvertResultModel<LiveRoomStreamDataInfo>();
                    if (resultModel.Code == 0)
                    {
                        return resultModel.Data;
                    }
                    else
                    {
                        throw new Exception($"Get user live room id failed. error code is {resultModel.Code}({resultModel.Msg}).");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// 开始直播
        /// </summary>
        /// <param name="user">用户</param>
        /// <param name="roomId">房间号</param>
        /// <param name="liveCategory">直播类型 默认户外</param>
        /// <returns></returns>
        public async Task<StartLiveDataInfo> StartLive(User user, string liveCategory = "123")
        {
            if (user == null || !user.IsLogin)
            {
                throw new Exception("User unlogin.");
            }
            try
            {
                string roomId = await GetRoomId(user);
                var queries = new QueryCollection {
                    { "room_id", roomId },
                    { "platform","pc" },
                    { "area_v2",liveCategory },
                    { "csrf_token",user.Data.Csrf },
                    { "csrf",user.Data.Csrf }
                };
                using (HttpResponseMessage response = await user.Client.SendAsync(HttpMethod.Post, StartLiveApi, queries, user.AppHeaders, user.Data.Cookie))
                {
                    ResultModel<StartLiveDataInfo> resultModel = await response.ConvertResultModel<StartLiveDataInfo>();
                    if (resultModel.Code == 0)
                    {
                        resultModel.Data.RoomId = roomId;
                        return resultModel.Data;
                    }
                    else
                    {
                        throw new Exception($"Start live failed. error code is {resultModel.Code}({resultModel.Msg}).");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
