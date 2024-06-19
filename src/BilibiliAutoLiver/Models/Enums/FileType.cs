using System.ComponentModel;

namespace BilibiliAutoLiver.Models.Enums
{
    public enum FileType
    {
        [Description("未知")]
        Unknow = 0,

        [Description("视频")]
        Video = 1,

        [Description("音频")]
        Music = 2
    }
}
