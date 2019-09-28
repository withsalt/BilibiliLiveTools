using System;
using System.Extensions;
using System.Net.Http;
using System.Threading.Tasks;
using Bilibili.Helper;
using Bilibili.Model;
using Bilibili.Model.Live;
using Bilibili.Model.Live.LiveCategoryInfo;
using Bilibili.Model.Live.LiveRoomInfo;
using Bilibili.Model.Live.LiveRoomStreamInfo;
using Bilibili.Model.Live.StartLiveInfo;
using Bilibili.Model.Live.StopLiveInfo;
using Bilibili.Settings;
using Bilibili.System.Extensions;
using Newtonsoft.Json.Linq;

namespace Bilibili.Api
{
    /// <summary>
    /// 直播API
    /// </summary>
    public class LiveApi
    {
        #region Api Url

        private const string _dynRoomsUrl = "http://room.lc4t.cn:8000/dyn_rooms/";

        /// <summary>
        /// 获取直播房间号
        /// </summary>
        private const string _getRoomIdApi = "https://api.live.bilibili.com/live_user/v1/UserInfo/live_info";

        /// <summary>
        /// 获取直播信息接口
        /// </summary>
        private const string _getStreamByRoomIdApi = "https://api.live.bilibili.com/live_stream/v1/StreamList/get_stream_by_roomId";

        /// <summary>
        /// 开始直播的API
        /// </summary>
        private const string _startLiveApi = "https://api.live.bilibili.com/room/v1/Room/startLive";

        /// <summary>
        /// 停止直播
        /// </summary>
        private const string _stopLiveApi = "https://api.live.bilibili.com/room/v1/Room/stopLive";

        /// <summary>
        /// 修改直播间名称
        /// </summary>
        private const string _updateLiveRoomNameApi = "https://api.live.bilibili.com/room/v1/Room/update";

        /// <summary>
        /// 获取直播种类
        /// </summary>
        private const string _getLiveCategoryApi = "https://api.live.bilibili.com/room/v1/Area/getList";

        #endregion

        #region 获取信息

        /// <summary>
        /// 动态获取房间ID列表
        /// </summary>
        /// <param name="start">起始序号（闭区间）</param>
        /// <param name="end">终止序号（开区间）</param>
        /// <returns></returns>
        public static async Task<uint[]> GetRoomIdsDynamicAsync(uint start, uint end)
        {
            using HttpClient client = new HttpClient();
            using HttpResponseMessage response = await client.SendAsync(HttpMethod.Get, _dynRoomsUrl + start.ToString() + "-" + end.ToString());
            return JObject.Parse(await response.Content.ReadAsStringAsync())["roomid"].ToObject<uint[]>();
        }

        /// <summary>
        /// 获取直播房间号
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<string> GetRoomId(User user)
        {
            if (user == null || !user.IsLogin)
            {
                throw new Exception("User unlogin.");
            }
            try
            {
                using (HttpResponseMessage response = await user.Handler.SendAsync(HttpMethod.Get, _getRoomIdApi, null, user.AppHeaders, user.Data.Cookie))
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
        /// </summary>
        /// <returns></returns>
        public static async Task<LiveRoomStreamDataInfo> GetRoomInfo(User user)
        {
            if (user == null || !user.IsLogin)
            {
                throw new Exception("User unlogin.");
            }
            try
            {
                string roomId = await GetRoomId(user);
                return await GetRoomInfo(user, roomId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取房间信息
        /// </summary>
        /// <returns></returns>
        public static async Task<LiveRoomStreamDataInfo> GetRoomInfo(User user, string roomId)
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
                using (HttpResponseMessage response = await user.Handler.SendAsync(HttpMethod.Get, _getStreamByRoomIdApi, queries, user.AppHeaders, user.Data.Cookie))
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
        /// 获取直播分类列表
        /// </summary>
        /// <returns></returns>
        public static async Task<LiveCategoryDataInfo> GetLiveCategoryInfo()
        {
            try
            {
                using HttpClient client = new HttpClient();
                using (HttpResponseMessage response = await client.SendAsync(HttpMethod.Get, _getLiveCategoryApi, null, GlobalSettings.Bilibili.PCHeaders))
                {
                    string result = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(result))
                    {
                        throw new Exception("result data is null.");
                    }
                    LiveCategoryDataInfo resultModel = JsonHelper.DeserializeJsonToObject<LiveCategoryDataInfo>(result);
                    if (resultModel.Code == 0)
                    {
                        return resultModel;
                    }
                    else
                    {
                        throw new Exception($"Get live category failed. error code is {resultModel.Code}({resultModel.Msg}).");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region 直播操作

        /// <summary>
        /// 开始直播
        /// </summary>
        /// <param name="user">用户</param>
        /// <param name="roomId">房间号</param>
        /// <param name="liveCategory">直播类型 默认户外</param>
        /// <returns></returns>
        public static async Task<StartLiveDataInfo> StartLive(User user, string liveCategory = "123")
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
                using (HttpResponseMessage response = await user.Handler.SendAsync(HttpMethod.Post, _startLiveApi, queries, user.AppHeaders, user.Data.Cookie))
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

        /// <summary>
        /// 停止直播
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<bool> StopLive(User user)
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
                    { "csrf_token",user.Data.Csrf },
                    { "csrf",user.Data.Csrf }
                };
                using (HttpResponseMessage response = await user.Handler.SendAsync(HttpMethod.Post, _stopLiveApi, queries, user.AppHeaders, user.Data.Cookie))
                {
                    ResultModel<StopLiveDataInfo> resultModel = await response.ConvertResultModel<StopLiveDataInfo>();
                    if (resultModel.Code == 0)
                    {
                        return true;
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

        /// <summary>
        /// 修改直播间名称
        /// </summary>
        /// <param name="user"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static async Task<bool> UpdateLiveRoomName(User user, string name)
        {
            if (user == null || !user.IsLogin)
            {
                throw new Exception("User unlogin.");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("Live name can not null.");
            }
            if (name.Length > 30)
            {
                throw new Exception("Live name length cannot more than 30.");
            }
            try
            {
                string roomId = await GetRoomId(user);
                var queries = new QueryCollection {
                    { "room_id", roomId },
                    { "title",name },
                    { "csrf_token",user.Data.Csrf },
                    { "csrf",user.Data.Csrf }
                };
                using (HttpResponseMessage response = await user.Handler.SendAsync(HttpMethod.Post, _updateLiveRoomNameApi, queries, user.AppHeaders, user.Data.Cookie))
                {
                    ResultModel<StartLiveDataInfo> resultModel = await response.ConvertResultModel<StartLiveDataInfo>();
                    if (resultModel.Code == 0)
                    {
                        return true;
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

        #endregion
    }
}
