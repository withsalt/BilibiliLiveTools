using System;

namespace Bilibili.AspNetCore.Apis.Exceptions
{
    public class CookieException : Exception
    {
        public CookieException(string message) : base(message)
        {

        }

        public CookieException(string message, Exception ex) : base(message, ex)
        {

        }
    }
}
