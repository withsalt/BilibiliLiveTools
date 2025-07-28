using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Models;
using Bilibili.AspNetCore.Apis.Models.Base;

namespace Bilibili.AspNetCore.Apis.Interface
{
    public interface IBilibiliAccountApiService
    {
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns></returns>
        Task<UserInfo> GetUserInfo(bool withCache = true);

        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        Task<UserInfo> LoginByCookie();

        /// <summary>
        /// 通过APP扫描二维码登录
        /// </summary>
        /// <returns></returns>
        Task<UserInfo> LoginByQrCode();

        /// <summary>
        /// 尝试获取二维码扫描登录状态
        /// 如果能获取到，表示正在进行二维码扫码登录
        /// </summary>
        /// <param name="loginStatus"></param>
        /// <returns></returns>
        bool TryGetQrCodeLoginStatus(out QrCodeLoginStatus loginStatus);

        /// <summary>
        /// 生成登录二维码
        /// </summary>
        /// <returns></returns>
        Task<QrCodeUrl> GenerateQrCode();

        /// <summary>
        /// 二维码是否扫描
        /// </summary>
        /// <param name="qrCodeKey"></param>
        /// <returns></returns>
        Task<ResultModel<QrCodeScanResult>> QrCodeScanStatus(string qrCodeKey);

        /// <summary>
        /// 心跳
        /// </summary>
        /// <returns></returns>
        Task HeartBeat();

        Task<bool> IsLogged();

        void Logout();

        /// <summary>
        /// 刷新Cookie
        /// </summary>
        /// <returns></returns>
        Task<bool> RefreshCookie();

        /// <summary>
        /// 是否需要刷新Cookie
        /// </summary>
        /// <returns></returns>
        Task<bool> CookieNeedToRefresh();
    }
}
