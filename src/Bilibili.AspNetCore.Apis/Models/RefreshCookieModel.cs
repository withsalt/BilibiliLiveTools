namespace Bilibili.AspNetCore.Apis.Models
{
    public class RefreshCookieModel
    {
        public string csrf { get; set; }
        public string refresh_csrf { get; set; }
        public string source { get; set; } = "main_web";
        public string refresh_token { get; set; }
    }
}
