using System.Threading.Tasks;
using BilibiliLiveCommon.Model;

namespace BilibiliLiveCommon.FFMpeg
{
    public interface IFFMpegService
    {
        Task<bool> Snapshot(string filePath, string outPath, int width, int height, int cutTime);

        string GetBinaryPath();

        Task<LibVersion> GetVersion();
    }
}
