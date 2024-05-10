using System;
using FFMpegCore;

namespace BilibiliAutoLiver.Services.FFMpeg.SourceReaders
{
    public interface ISourceReader : IDisposable
    {
        ISourceReader WithInputArg();

        FFMpegArgumentProcessor WithOutputArg();
    }
}
