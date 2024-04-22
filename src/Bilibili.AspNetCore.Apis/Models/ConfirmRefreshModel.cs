namespace Bilibili.AspNetCore.Apis.Models
{
    public class ConfirmRefreshModel
    {
        public string csrf { get; set; }
        public string refresh_token { get; set; }
    }
}
