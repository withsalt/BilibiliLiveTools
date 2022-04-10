using BilibiliLiver.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BilibiliLiver.Services.Interface
{
    public interface IBilibiliLiveApiService
    {
        /// <summary>
        /// 获取直播间信息
        /// </summary>
        /// <returns></returns>
        Task<LiveRoomInfo> GetLiveRoomInfo();

        /// <summary>
        /// 更新直播间房间号
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<bool> UpdateLiveRoomName(int roomId, string title);

        /// <summary>
        /// 获取直播间分类
        /// </summary>
        /// <returns></returns>
        Task<List<LiveCategoryItem>> GetLiveCategories();

        /// <summary>
        /// 开始直播
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        Task<StartLiveInfo> StartLive(int roomId, string categoryId);
    }
}
