using System;
using BilibiliAutoLiver.Models.Dtos.Common.JsonObject;

namespace BilibiliAutoLiver.Models.Dtos.Common
{
    public class ResultModel<T> : IRoot<T> where T : class
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

        public int Code { get; set; }

        public string Message { get; set; }

        public T Data { get; set; }

        public DateTime Time { get; } = DateTime.Now;
    }
}
