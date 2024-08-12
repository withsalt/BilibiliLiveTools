using System.ComponentModel;

namespace BilibiliAutoLiver.Models.Enums
{
    /// <summary>
    /// 推流输出质量
    /// </summary>
    public enum OutputQualityEnum
    {
        [Description("高")]
        High = 1,

        [Description("中等")]
        Medium = 2,

        [Description("低")]
        Low = 3,

        [Description("原画")]
        Original = 9,
    }
}
