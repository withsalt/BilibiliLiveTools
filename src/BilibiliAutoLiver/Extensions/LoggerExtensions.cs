using Microsoft.Extensions.Logging;
using System;

namespace BilibiliAutoLiver.Extensions
{
    public static class LoggerExtensions
    {
        public static void ThrowLogError(this ILogger logger, string message)
        {
            logger.LogError(message);
            throw new Exception(message);
        }

        public static void ThrowLogError(this ILogger logger, string message, Exception ex)
        {
            logger.LogError(message);
            throw new Exception(message, ex);
        }
    }
}
