using BilibiliLiver.Model;
using System.Threading.Tasks;

namespace BilibiliLiver.Services.Interface
{
    public interface IAccountService
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        Task<UserInfo> Login();

        /// <summary>
        /// 心跳
        /// </summary>
        /// <returns></returns>
        Task HeartBeat();
    }
}
