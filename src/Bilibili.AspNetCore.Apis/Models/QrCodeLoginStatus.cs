namespace Bilibili.AspNetCore.Apis.Models
{
    public class QrCodeLoginStatus
    {
        public bool IsLogged { get; set; }

        public bool IsScaned { get; set; }

        public string QrCode { get; set; }

        public int QrCodeEffectiveTime { get; set; }

        public int Index { get; set; }
    }
}
