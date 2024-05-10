using FFMpegCore;

namespace BilibiliAutoLiver.Services.SourceReaders
{
    public interface ISourceReader
    {
        ISourceReader WithInputArg();

        FFMpegArgumentProcessor WithOutputArg();
    }
}
