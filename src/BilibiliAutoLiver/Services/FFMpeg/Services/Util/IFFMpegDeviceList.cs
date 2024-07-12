using System.Collections.Generic;
using System.Threading.Tasks;

namespace BilibiliAutoLiver.Services.FFMpeg.Services.Util
{
    public interface IFFMpegDeviceList
    {
        Task<List<string>> GetVideoDevices();

        Task<List<string>> GetAudioDevices();
    }
}
