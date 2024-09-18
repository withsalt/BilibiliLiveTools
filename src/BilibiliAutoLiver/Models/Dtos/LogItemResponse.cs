using System;
using BilibiliAutoLiver.Models.Enums;

namespace BilibiliAutoLiver.Models.Dtos
{
    public class LogItemResponse
    {
        public LogType LogType { get; set; }

        public DateTime Time { get; set; }

        public string Message { get; set; }

        public string StackTrace { get; set; }
    }
}
