using FFMpegCore;

namespace BilibiliAutoLiver.Services.FFMpeg.SourceReaders
{
    public interface ISourceReader
    {
        ISourceReader WithInputArg();

        FFMpegArgumentProcessor WithOutputArg();
    }
}
