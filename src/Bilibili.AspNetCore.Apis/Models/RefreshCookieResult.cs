namespace Bilibili.AspNetCore.Apis.Models
{
    public class RefreshCookieResult
    {
        public int status { get; set; }
        public string message { get; set; }
        public string refresh_token { get; set; }
    }
}
