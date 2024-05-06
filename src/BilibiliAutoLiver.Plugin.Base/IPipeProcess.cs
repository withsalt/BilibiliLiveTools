using SkiaSharp;

namespace BilibiliAutoLiver.Plugin.Base
{
    public interface IPipeProcess
    {
        /// <summary>
        /// 插件执行序号
        /// </summary>
        int Index { get; }

        SKBitmap Process(SKBitmap bitmap);
    }
}
