using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliAutoLiver.Exceptions
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
