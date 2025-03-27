using System;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models.Enums;

namespace BilibiliAutoLiver.Services.Interface
{
    public interface IPushStreamService : IDisposable
    {
        /// <summary>
        /// 开启推流
        /// </summary>
        /// <returns></returns>
        Task<bool> Start(bool isStartup);

        /// <summary>
        /// 结束推流
        /// </summary>
        /// <returns></returns>
        Task<bool> Stop();

        /// <summary>
        /// 获取当前推流状态
        /// </summary>
        /// <returns></returns>
        PushStatus GetStatus();

        /// <summary>
        /// 测试ffmpeg
        /// </summary>
        /// <returns></returns>
        Task CheckFFmpegBinary();

        /// <summary>
        /// 检查直播配置
        /// </summary>
        Task CheckLiveSetting();

        /// <summary>
        /// 检查直播间配置
        /// </summary>
        /// <returns></returns>
        Task CheckLiveRoom();
    }
}
