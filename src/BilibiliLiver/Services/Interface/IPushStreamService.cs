using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLiver.Services.Interface
{
    public interface IPushStreamService
    {
        /// <summary>
        /// 测试ffmpeg
        /// </summary>
        /// <returns></returns>
        Task<bool> FFmpegTest();
    }
}
