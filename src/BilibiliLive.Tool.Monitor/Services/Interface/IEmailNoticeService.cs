using System.Threading.Tasks;

namespace BilibiliLive.Tool.Monitor.Services.Interface
{
    public interface IEmailNoticeService
    {
        Task<(SendStatus, string)> Send(string title
            , string body
            , string[] attachments = null
            , bool isBodyHtml = false);
    }
}
