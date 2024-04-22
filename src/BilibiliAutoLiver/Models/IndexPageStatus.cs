using Bilibili.AspNetCore.Apis.Models;

namespace BilibiliAutoLiver.Models
{
    public class IndexPageStatus
    {
        public QrCodeLoginStatus LoginStatus { get; set; }

        public UserInfo UserInfo { get; set; }
    }
}
