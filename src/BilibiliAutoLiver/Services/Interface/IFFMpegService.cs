using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models;
using BilibiliAutoLiver.Models.FFMpeg;
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

        Task<List<DeviceResolution>> ListVideoDeviceSupportResolutions(string deviceName);

        #region Log

        IEnumerable<FFMpegLog> GetLogs();

        void AddLog(DateTime time, string message);

        void AddLog(DateTime time, string message, Exception ex);

        void ClearLog();

        #endregion
    }
}
