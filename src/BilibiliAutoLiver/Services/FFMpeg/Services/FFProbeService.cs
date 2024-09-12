using System;
using System.IO;
using System.Threading.Tasks;
using BilibiliAutoLiver.Services.Base;
using BilibiliAutoLiver.Services.Interface;
using FFMpegCore;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Services.FFMpeg.Services
{
    public class FFProbeService : BaseFFPlayService, IFFProbeService
    {
        private readonly ILogger<FFProbeService> _logger;

        public FFProbeService(ILogger<FFProbeService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IMediaAnalysis> Analyse(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(nameof(filePath));
            }
            IMediaAnalysis mediaInfo = await FFProbe.AnalyseAsync(filePath);
            return mediaInfo;
        }
    }
}
