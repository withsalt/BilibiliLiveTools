using Bilibili.AspNetCore.Apis.Models;

namespace BilibiliAutoLiver.Models.Dtos
{
    public class BilibiliAccountLoginStatus
    {
        public QrCodeLoginStatus QrCodeStatus { get; set; }

        public AccountLoginStatus Status { get; set; }

        public string RedirectUrl { get; set; }
    }

    public enum AccountLoginStatus
    {
        NotLogin = 1,

        Logging = 2,

        Logged = 3,

        Waiting = 4,
    }
}
