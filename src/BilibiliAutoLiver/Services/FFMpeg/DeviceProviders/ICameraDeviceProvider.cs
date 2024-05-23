using System;
using System.Drawing;
using System.Threading.Tasks;

namespace BilibiliAutoLiver.Services.FFMpeg.DeviceProviders
{
    public interface ICameraDeviceProvider : IDisposable
    {
        Size Size { get; }

        Task Start();

        Task Stop();
    }
}
