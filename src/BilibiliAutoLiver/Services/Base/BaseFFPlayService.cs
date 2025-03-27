using FFMpegCore;

namespace BilibiliAutoLiver.Services.Base
{
    public abstract class BaseFFPlayService
    {
        public string GetBinaryFolder()
        {
            return GlobalFFOptions.Current.BinaryFolder;
        }

        public string GeTemporaryFilesFolder()
        {
            return GlobalFFOptions.Current.TemporaryFilesFolder;
        }
    }
}
