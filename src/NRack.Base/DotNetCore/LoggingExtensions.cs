using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Logging
{
    public static class LoggingExtensions
    {
        public static void ErrorFormat(this ILogger logger, string message, params string[] args)
        {
            logger.LogError(null, message, args);
        }
        
        public static void Error(this ILogger logger, string message, Exception e)
        {
            logger.LogError(null, e, message);
        }
        
        public static void Error(this ILogger logger, string message)
        {
            logger.LogError(null, message);
        }
    }
}