using System;
using System.Threading.Tasks;

namespace BilibiliAutoLiver.Services.FFMpeg.DeviceProviders
{
    public interface ICameraDeviceProvider : IDisposable
    {
        Task Start();

        Task Stop();
    }
}
