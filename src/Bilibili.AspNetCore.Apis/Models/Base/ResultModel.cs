using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;

namespace Bilibili.AspNetCore.Apis.Models.Base
{
    public class ResultModel<T> where T : class
    {
        public ResultModel()
        {

        }

        public ResultModel(int code)
        {
            Code = code;
            if (code == 0)
            {
                Message = "Success";
            }
        }

        public ResultModel(int code, string message)
        {
            Code = code;
            Message = message;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Code { get; set; } = int.MinValue;

        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Cookies
        /// </summary>
        public IEnumerable<CookieHeaderValue> Cookies { get; set; }
    }
}
