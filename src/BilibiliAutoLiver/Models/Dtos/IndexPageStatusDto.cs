using Bilibili.AspNetCore.Apis.Models;

namespace BilibiliAutoLiver.Models.Dtos
{
    public class IndexPageStatusDto
    {
        public QrCodeLoginStatus LoginStatus { get; set; }

        public UserInfo UserInfo { get; set; }

        public MyLiveRoomInfo LiveRoomInfo { get; set; }
    }
}
