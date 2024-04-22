using System;

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
