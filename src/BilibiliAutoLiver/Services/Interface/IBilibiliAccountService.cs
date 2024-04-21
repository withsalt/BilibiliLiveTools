using BilibiliAutoLiver.Model;
using BilibiliAutoLiver.Model.Base;
using System.Threading.Tasks;

namespace BilibiliAutoLiver.Services.Interface
{
    public interface IBilibiliAccountService
    {
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns></returns>
        Task<UserInfo> GetUserInfo();

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
