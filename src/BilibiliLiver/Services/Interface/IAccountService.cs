using BilibiliLiver.Model;
using System.Threading.Tasks;

namespace BilibiliLiver.Services.Interface
{
    public interface IAccountService
    {
        Task<UserInfo> Login();
    }
}
