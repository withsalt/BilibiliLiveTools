using System;
using System.Collections.Generic;
using System.Text;

namespace Bilibili.Model.User.LoginResult
{
    class RefreshTokenResult
    {
        /// <summary>
        /// 
        /// </summary>
        public int Ts { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public RefreshTokenResultData Data { get; set; }
    }
}
