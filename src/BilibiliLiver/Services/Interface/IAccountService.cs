using BilibiliLiver.Model;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLiver.Services
{
    public interface IAccountService
    {
        Task<UserInfo> Login();
    }
}
