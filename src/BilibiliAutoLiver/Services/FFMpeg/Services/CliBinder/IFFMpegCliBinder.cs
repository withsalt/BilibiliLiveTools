using System.Collections.Generic;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models;
using BilibiliAutoLiver.Models.FFMpeg;

namespace BilibiliAutoLiver.Services.FFMpeg.Services.CliBinder
{
    public interface IFFMpegCliBinder
    {
        Task<LibVersion> GetVersion();

        Task<List<string>> GetVideoDevices();

        Task<List<string>> GetAudioDevices();

        Task<List<DeviceResolution>> ListVideoDeviceSupportResolutions(string deviceName);
    }
}
