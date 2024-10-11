using System;
using Microsoft.Extensions.Logging;

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

        public static void ThrowLogWarning(this ILogger logger, string message)
        {
            logger.LogWarning(message);
            throw new NotSupportedException(message);
        }

        public static void ThrowLogWarning(this ILogger logger, string message, Exception ex)
        {
            logger.LogWarning(message);
            throw new NotSupportedException(message, ex);
        }
    }
}
