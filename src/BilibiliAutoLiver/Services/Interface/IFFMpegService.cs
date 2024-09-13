using System.Collections.Generic;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models;
using FFMpegCore.Enums;

namespace BilibiliAutoLiver.Services.Interface
{
    public interface IFFMpegService
    {
        Task<bool> Snapshot(string filePath, string outPath, int width, int height, int cutTime);

        string GetBinaryPath();

        Task<LibVersion> GetVersion();

        Task<List<string>> GetVideoDevices();

        Task<List<string>> GetAudioDevices();

        IReadOnlyList<Codec> GetVideoCodecs();

        Task<List<string>> ListVideoDeviceSupportResolutions(string deviceName);
    }
}
