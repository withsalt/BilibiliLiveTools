using System.Threading.Tasks;
using FFMpegCore;

namespace BilibiliAutoLiver.Services.Interface
{
    public interface IFFProbeService
    {
        Task<IMediaAnalysis> Analyse(string filePath);
    }
}
