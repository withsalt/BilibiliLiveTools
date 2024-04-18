using BilibiliLiveCommon.Model;
using BilibiliLiveCommon.Model.Base;
using System.Threading.Tasks;

namespace BilibiliLiveCommon.Services.Interface
{
    public interface IBilibiliAccountService
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        Task<UserInfo> LoginByCookie();

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
        Task<ResultModel<QrCodeScanResult>> QrCodeHasScaned(string qrCodeKey);

        /// <summary>
        /// 心跳
        /// </summary>
        /// <returns></returns>
        Task HeartBeat();
    }
}
