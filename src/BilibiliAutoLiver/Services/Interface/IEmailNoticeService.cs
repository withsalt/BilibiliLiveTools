using System.Threading.Tasks;

namespace BilibiliAutoLiver.Services.Interface
{
    public interface IEmailNoticeService
    {
        Task<(SendStatus, string)> Send(string title, string body);
    }
}
