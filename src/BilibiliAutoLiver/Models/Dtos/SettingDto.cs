using BilibiliAutoLiver.Models.Entities;

namespace BilibiliAutoLiver.Models.Dtos
{
    public class SettingDto
    {
        public LiveSetting LiveSetting { get; set; }

        public PushSettingDto PushSetting { get; set; }
    }
}
