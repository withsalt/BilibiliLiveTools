using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FFMpegCore.Pipes;

namespace BilibiliAutoLiver.Services.FFMpeg.PipeSource
{
    public class VideoFilePipeSource : IPipeSource
    {
        public string GetStreamArguments()
        {
            throw new System.NotImplementedException();
        }

        public Task WriteAsync(Stream outputStream, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
