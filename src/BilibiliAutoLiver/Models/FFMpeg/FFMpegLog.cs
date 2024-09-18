using System;
using BilibiliAutoLiver.Models.Enums;

namespace BilibiliAutoLiver.Models.FFMpeg
{
    public class FFMpegLog
    {
        public FFMpegLog(LogType logType, string message, Exception exception)
        {
            this.LogType = logType;
            this.Time = DateTime.UtcNow;
            this.Message = message;
            this.Exception = exception;
        }

        public LogType LogType { get; set; }

        public DateTime Time { get; set; }

        public string Message { get; set; }

        public Exception Exception { get; set; }
    }
}
