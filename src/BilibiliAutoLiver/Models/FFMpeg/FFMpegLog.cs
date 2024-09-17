using System;

namespace BilibiliAutoLiver.Models.FFMpeg
{
    public class FFMpegLog
    {
        public FFMpegLog(DateTime time, string message, Exception exception)
        {
            this.Time = time;
            this.Message = message;
            Exception = exception;
        }

        public DateTime Time { get; set; }

        public string Message { get; set; }

        public Exception Exception { get; set; }
    }
}
