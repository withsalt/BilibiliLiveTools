using System;
using BilibiliAutoLiver.Models.Enums;

namespace BilibiliAutoLiver.Models.Dtos
{
    public class LogItemResponse
    {
        public DateTime Time { get; set; }

        public string Message { get; set; }

        public string StackTrace { get; set; }
    }
}
