using System;
using System.Collections.Generic;
using System.Text;

namespace Bilibili.Model.User.LoginResult
{
    public class CookieInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public List<CookiesItem> Cookies { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> Domains { get; set; }
    }
}
