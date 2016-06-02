using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Logging
{
    public static class LoggingExtensions
    {
        public static void ErrorFormat(this ILogger logger, string message, params string[] args)
        {
            logger.LogError(null, message, args);
        }
    }
}