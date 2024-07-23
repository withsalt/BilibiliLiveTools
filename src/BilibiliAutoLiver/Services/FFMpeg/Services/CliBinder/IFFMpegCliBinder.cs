using System.Collections.Generic;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models;

namespace BilibiliAutoLiver.Services.FFMpeg.Services.CliBinder
{
    public interface IFFMpegCliBinder
    {
        Task<LibVersion> GetVersion();

        Task<List<string>> GetVideoDevices();

        Task<List<string>> GetAudioDevices();
    }
}
