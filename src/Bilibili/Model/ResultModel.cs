using Bilibili.Model.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bilibili.Model
{
    public class ResultModel<T> where T : IResultData
    {
        /// <summary>
        /// 
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Msg { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public T Data { get; set; }
    }
}
