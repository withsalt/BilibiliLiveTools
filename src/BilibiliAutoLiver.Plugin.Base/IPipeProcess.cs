using SkiaSharp;

namespace BilibiliAutoLiver.Plugin.Base
{
    public interface IPipeProcess
    {
        /// <summary>
        /// 插件执行序号
        /// </summary>
        int Index { get; }

        /// <summary>
        /// 插件名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 是否启用
        /// </summary>
        bool IsEnabled { get; set; }

        SKBitmap Process(SKBitmap bitmap);
    }
}
