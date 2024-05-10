using System.Threading.Tasks;

namespace BilibiliAutoLiver.Services.FFMpeg.DeviceProviders
{
    public interface ICameraDeviceProvider
    {
        Task Start();
    }
}
