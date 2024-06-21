using System.ComponentModel;

namespace BilibiliAutoLiver.Models.Enums
{
    public enum PushStatus
    {
        [Description("启动中")]
        Starting = 1,

        [Description("已停止")]
        Stopped = 2,

        [Description("运行中")]
        Running = 3,

        [Description("等待中")]
        Waiting = 4,
    }
}
