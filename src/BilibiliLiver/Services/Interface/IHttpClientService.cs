using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLiver.Services
{
    public interface IHttpClientService
    {
        Task<T> Execute<T>(string url, Method method, object body = null) where T : class;
    }
}
