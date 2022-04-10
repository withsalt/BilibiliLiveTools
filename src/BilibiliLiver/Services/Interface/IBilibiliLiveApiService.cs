using BilibiliLiver.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLiver.Services
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
    }
}
