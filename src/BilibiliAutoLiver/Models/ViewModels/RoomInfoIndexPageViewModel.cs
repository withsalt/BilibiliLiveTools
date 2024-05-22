using System.Collections.Generic;
using Bilibili.AspNetCore.Apis.Models;
using BilibiliAutoLiver.Models.Entities;

namespace BilibiliAutoLiver.Models.ViewModels
{
    public class RoomInfoIndexPageViewModel
    {
        public MyLiveRoomInfo LiveRoomInfo { get; set; }

        public List<LiveAreaItem> LiveAreas { get; set; }

        public LiveSetting LiveSetting { get; set; }
    }
}
