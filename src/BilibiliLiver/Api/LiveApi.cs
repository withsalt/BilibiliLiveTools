using System;
using System.Extensions;
using System.Net.Http;
using System.Threading.Tasks;
using BiliAccount;
using BilibiliLiver.Model;
using BilibiliLiver.Model.Interface;
using BilibiliLiver.Model.Live;
using BilibiliLiver.System.Extensions;
using BilibiliLiver.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace BilibiliLiver.Api
{
    /// <summary>
    /// 直播API
    /// </summary>
    public class LiveApi : IBaseApi
    {
        public LiveApi()
        {
            
        }

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

        /// <summary>
        /// 获取直播间信息
        /// </summary>
        private const string _getUserRoomInfoApi = "https://api.live.bilibili.com/room/v1/Room/get_info";

        /// <summary>
        /// 获取直播间真实流地址
        /// </summary>
        private const string _getRealRoomAddressApi = "https://api.live.bilibili.com/room/v1/Room/playUrl";

        /// <summary>
        /// 更新直播间直播地址
        /// </summary>
        private const string _updateLiveRoomCategory = "https://api.live.bilibili.com/room/v1/Room/update";

        #endregion

        #region 获取信息

        /// <summary>
        /// 动态获取房间ID列表
        /// </summary>
        /// <param name="start">起始序号（闭区间）</param>
        /// <param name="end">终止序号（开区间）</param>
        /// <returns></returns>
        public async Task<uint[]> GetRoomIdsDynamicAsync(uint start, uint end)
        {
            using (HttpClient client = new HttpClient())
            {
                using HttpResponseMessage response = await client.SendAsync(HttpMethod.Get, _dynRoomsUrl + start.ToString() + "-" + end.ToString());
                return JObject.Parse(await response.Content.ReadAsStringAsync())["roomid"].ToObject<uint[]>();
            }
        }

        /// <summary>
        /// 获取直播房间号
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<string> GetRoomId(Account user)
        {
            if (user == null)
            {
                throw new Exception("User unlogin.");
            }
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.SendAsync(HttpMethod.Get, _getRoomIdApi, null, this.PCHeaders, GetCookie(user)))
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
        }

        /// <summary>
        /// 获取房间信息
        /// 用户登录后
        /// </summary>
        /// <returns></returns>
        public async Task<LiveRoomStreamDataInfo> GetRoomInfo(Account user)
        {
            if (user == null)
            {
                throw new Exception("User unlogin.");
            }
            string roomId = await GetRoomId(user);
            return await GetRoomInfo(user, roomId);
        }

        /// <summary>
        /// 更具直播间ID获取房间信息
        /// 用户登录后
        /// </summary>
        /// <returns></returns>
        public async Task<LiveRoomStreamDataInfo> GetRoomInfo(Account user, string roomId)
        {
            if (user == null)
            {
                throw new Exception("User unlogin.");
            }
            if (string.IsNullOrEmpty(roomId))
            {
                throw new Exception("Room id cannot null.");
            }
            var queries = new QueryCollection {
                    { "room_id", roomId }
            };
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.SendAsync(HttpMethod.Get, _getStreamByRoomIdApi, queries, this.PCHeaders, GetCookie(user)))
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
        }

        /// <summary>
        /// 更具直播间ID获取房间信息（不需要登录），获取到的为直播间详细信息
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public async Task<UserLiveRoomDataInfo> GetRoomInfo(string roomId)
        {
            if (string.IsNullOrEmpty(roomId))
            {
                throw new Exception("Room id cannot null.");
            }
            var queries = new QueryCollection {
                    { "id", roomId }
            };
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.SendAsync(HttpMethod.Get, _getUserRoomInfoApi, queries, this.PCHeaders))
                {
                    ResultModel<UserLiveRoomDataInfo> resultModel = await response.ConvertResultModel<UserLiveRoomDataInfo>();
                    if (resultModel.Code == 0)
                    {
                        return resultModel.Data;
                    }
                    else
                    {
                        throw new Exception($"Get live room info failed. error code is {resultModel.Code}({resultModel.Msg}).");
                    }
                }
            }
        }

        /// <summary>
        /// 获取直播分类列表
        /// </summary>
        /// <returns></returns>
        public async Task<LiveCategoryDataInfo> GetLiveCategoryInfo()
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.SendAsync(HttpMethod.Get, _getLiveCategoryApi, null, this.PCHeaders))
                {
                    string result = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(result))
                    {
                        throw new Exception("result data is null.");
                    }
                    LiveCategoryDataInfo resultModel = JsonUtil.DeserializeJsonToObject<LiveCategoryDataInfo>(result);
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
        }

        /// <summary>
        /// 获取直播间真实流地址
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public async Task<RealAddressDataInfo> GetLiveRealAddress(string roomId)
        {
            if (string.IsNullOrEmpty(roomId))
            {
                throw new Exception("Room id cannot null.");
            }
            var queries = new QueryCollection {
                    { "cid", roomId },
                    { "qn", "10000" },
                    { "platform", "web" }
            };
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.SendAsync(HttpMethod.Get, _getRealRoomAddressApi, queries, this.PCHeaders))
                {
                    ResultModel<RealAddressDataInfo> resultModel = await response.ConvertResultModel<RealAddressDataInfo>();
                    if (resultModel.Code == 0)
                    {
                        return resultModel.Data;
                    }
                    else
                    {
                        throw new Exception($"Get live room address failed. error code is {resultModel.Code}({resultModel.Msg}).");
                    }
                }
            }
        }

        /// <summary>
        /// 更新直播间分类
        /// </summary>
        /// <param name="user"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> UpdateLiveCategory(Account user, string id)
        {
            LiveCategoryDataInfo liveCategoryData = await GetLiveCategoryInfo();
            if (liveCategoryData == null || liveCategoryData.Code != 0 || liveCategoryData.Data == null)
            {
                return false;
            }
            bool isOk = false;
            foreach (var item in liveCategoryData.Data)
            {
                foreach (var cate in item.List)
                {
                    if (cate.id.Equals(id, StringComparison.OrdinalIgnoreCase))
                    {
                        isOk = true;
                    }
                }
            }
            if (!isOk)
            {
                return false;
            }

            string roomId = await GetRoomId(user);
            var queries = new QueryCollection {
                    { "room_id", roomId },
                    { "area_id",id },
                    { "csrf_token",user.CsrfToken },
                    { "csrf",user.CsrfToken }
                };
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.SendAsync(HttpMethod.Post, _updateLiveRoomCategory, queries, this.PCHeaders, GetCookie(user)))
                {
                    ResultModel<IResultData> resultModel = await response.ConvertResultModel<IResultData>();
                    if (resultModel.Code == 0)
                    {
                        return true;
                    }
                    else
                    {
                        throw new Exception($"Update live category failed. error code is {resultModel.Code}({resultModel.Msg}).");
                    }
                }
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
        public async Task<StartLiveDataInfo> StartLive(Account user, string liveCategory = "123")
        {
            if (user == null)
            {
                throw new Exception("User unlogin.");
            }
            string roomId = await GetRoomId(user);
            var queries = new QueryCollection {
                { "room_id", roomId },
                { "platform","pc" },
                { "area_v2",liveCategory },
                { "csrf_token",user.CsrfToken },
                { "csrf",user.CsrfToken }
            };
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.SendAsync(HttpMethod.Post, _startLiveApi, queries, this.PCHeaders, GetCookie(user)))
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
        }

        /// <summary>
        /// 停止直播
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<bool> StopLive(Account user)
        {
            if (user == null)
            {
                throw new Exception("User unlogin.");
            }
            string roomId = await GetRoomId(user);
            var queries = new QueryCollection {
                    { "room_id", roomId },
                    { "platform","pc" },
                    { "csrf_token",user.CsrfToken },
                    { "csrf",user.CsrfToken }
            };
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.SendAsync(HttpMethod.Post, _stopLiveApi, queries, this.PCHeaders, GetCookie(user)))
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
        }

        /// <summary>
        /// 修改直播间名称
        /// </summary>
        /// <param name="user"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<bool> UpdateLiveRoomName(Account user, string name)
        {
            if (user == null)
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
            string roomId = await GetRoomId(user);
            var queries = new QueryCollection {
                    { "room_id", roomId },
                    { "title",name },
                    { "csrf_token",user.CsrfToken },
                    { "csrf",user.CsrfToken }
                };
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.SendAsync(HttpMethod.Post, _updateLiveRoomNameApi, queries, this.PCHeaders, GetCookie(user)))
                {
                    ResultModel<StartLiveDataInfo> resultModel = await response.ConvertResultModel<StartLiveDataInfo>();
                    if (resultModel.Code == 0)
                    {
                        return true;
                    }
                    else
                    {
                        throw new Exception($"Update live room name failed. error code is {resultModel.Code}({resultModel.Msg}).");
                    }
                }
            }
        }

        #endregion
    }
}
