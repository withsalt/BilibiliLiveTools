using System.Collections.ObjectModel;
using System.Net.Http.Headers;

namespace Bilibili.AspNetCore.Apis.Models.Base
{
    public class ResultModel<T> where T : class
    {
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
        public ReadOnlyCollection<CookieHeaderValue> Cookies { get; set; }
    }
}
