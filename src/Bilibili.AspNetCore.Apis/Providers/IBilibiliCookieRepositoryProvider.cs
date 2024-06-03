using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bilibili.AspNetCore.Apis.Providers
{
    public interface IBilibiliCookieRepositoryProvider
    {
        /// <summary>
        /// 保存Cookie
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        Task Write(string cookie);

        /// <summary>
        /// 读取Cookie
        /// </summary>
        /// <returns></returns>
        Task<string> Read();

        /// <summary>
        /// 删除Cookie
        /// </summary>
        /// <returns></returns>
        Task Delete();
    }
}
