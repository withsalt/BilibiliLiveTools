﻿using System.Threading.Tasks;
using BilibiliAutoLiver.Models;

namespace BilibiliAutoLiver.Services.Interface
{
    public interface IFFMpegService
    {
        Task<bool> Snapshot(string filePath, string outPath, int width, int height, int cutTime);

        string GetBinaryPath();

        Task<LibVersion> GetVersion();
    }
}