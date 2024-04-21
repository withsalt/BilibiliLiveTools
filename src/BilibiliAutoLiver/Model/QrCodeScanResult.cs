using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliAutoLiver.Model
{
    public class QrCodeScanResult
    {
        public string url { get; set; }
        public string refresh_token { get; set; }
        public long timestamp { get; set; }
        public int code { get; set; }
        public string message { get; set; }

        /// <summary>
        /// 二维码扫描状态
        /// </summary>
        public QrCodeStatus status
        {
            get
            {
                switch (code)
                {
                    case 0:
                        return QrCodeStatus.Scaned;
                    case 86038:
                        return QrCodeStatus.Expired;
                    case 86101:
                        return QrCodeStatus.WaitingScan;
                    case 86090:
                        return QrCodeStatus.ScanedWithoutLogin;
                    default:
                        return QrCodeStatus.Unknow;
                }
            }
        }
    }

    public enum QrCodeStatus
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknow = 0,

        /// <summary>
        /// 已扫描
        /// </summary>
        Scaned,

        /// <summary>
        /// 已扫描，但是未确认
        /// </summary>
        ScanedWithoutLogin,

        /// <summary>
        /// 等待扫描
        /// </summary>
        WaitingScan,

        /// <summary>
        /// 已失效
        /// </summary>
        Expired
    }
}
