using Microsoft.Extensions.Logging;
using System;

namespace BilibiliLiveMonitor.Extensions
{
    public static class LoggerExtensions
    {
        public static void ThrowLogError(this ILogger logger, string message)
        {
            logger.LogError(message);
            throw new Exception(message);
        }
    }
}
