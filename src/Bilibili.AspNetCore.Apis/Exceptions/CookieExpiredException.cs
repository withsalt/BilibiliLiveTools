using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bilibili.AspNetCore.Apis.Exceptions
{
    public class CookieExpiredException : Exception
    {
        public CookieExpiredException(string message) : base(message)
        {

        }

        public CookieExpiredException(string message, Exception ex) : base(message, ex)
        {

        }
    }
}
