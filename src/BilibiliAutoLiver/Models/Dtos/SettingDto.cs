using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Models.Settings;

namespace BilibiliAutoLiver.Models.Dtos
{
    public class SettingDto
    {
        public LiveSetting LiveSetting { get; set; }

        public PushSettingDto PushSetting { get; set; }

        public AppSettings AppSettings { get; set; }
    }
}
