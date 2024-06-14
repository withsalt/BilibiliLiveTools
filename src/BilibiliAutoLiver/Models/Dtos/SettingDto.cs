using BilibiliAutoLiver.Models.Entities;

namespace BilibiliAutoLiver.Models.Dtos
{
    public class SettingDto
    {
        public PushSetting PushSetting { get; set; }

        public LiveSetting LiveSetting { get; set; }
    }
}
