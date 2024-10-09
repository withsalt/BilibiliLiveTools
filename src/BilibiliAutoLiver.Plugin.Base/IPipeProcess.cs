using System;
using SkiaSharp;

namespace BilibiliAutoLiver.Plugin.Base
{
    public interface IPipeProcess : IDisposable
    {
        /// <summary>
        /// 插件执行序号
        /// </summary>
        int Index { get; }

        /// <summary>
        /// 插件名称
        /// </summary>
        string Name { get; }

        SKBitmap Process(SKBitmap bitmap);
    }
}
