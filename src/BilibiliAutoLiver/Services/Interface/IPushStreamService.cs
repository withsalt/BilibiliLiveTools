using System.Threading.Tasks;

namespace BilibiliAutoLiver.Services.Interface
{
    public interface IPushStreamService
    {
        /// <summary>
        /// 开启推流
        /// </summary>
        /// <returns></returns>
        Task Start();

        /// <summary>
        /// 结束推流
        /// </summary>
        /// <returns></returns>
        Task Stop();

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
