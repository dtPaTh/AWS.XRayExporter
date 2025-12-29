using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace XRayConnector.Telemetry
{
    public static class LoggerDurableExtensions
    {
        /// <summary>
        /// Logs an information message if the orchestration is not replaying.
        /// </summary>
        public static void LogInformationOnce(
            this ILogger logger,
            TaskOrchestrationContext context,
            string message)
        {
            if (context == null || logger == null)
                return;

            if (!context.IsReplaying)
            {
                logger.LogInformation(message);
            }
        }

        /// <summary>
        /// Logs an information message with structured data if the orchestration is not replaying.
        /// </summary>
        public static void LogInformation(
            this ILogger logger,
            TaskOrchestrationContext context,
            string message,
            params object[] args)
        {
            if (context == null || logger == null)
                return;

            if (!context.IsReplaying)
            {
                logger.LogInformation(message, args);
            }
        }

        /// <summary>
        /// Logs an information message with an event id if the orchestration is not replaying.
        /// </summary>
        //public static void LogInformation(
        //    this ILogger logger,
        //    TaskOrchestrationContext context,
        //    EventId eventId,
        //    string message,
        //    params object[] args)
        //{
        //    if (context == null || logger == null)
        //        return;

        //    if (!context.IsReplaying)
        //    {
        //        logger.LogInformation(eventId, message, args);
        //    }
        //}

        public static void LogWarning(
            this ILogger logger,
            TaskOrchestrationContext context,
            string message)
        {
            if (context == null || logger == null)
                return;

            if (!context.IsReplaying)
            {
                logger.LogWarning(message);
            }
        }

        /// <summary>
        /// Logs a warning message with structured data if the orchestration is not replaying.
        /// </summary>
        //public static void LogWarning(
        //    this ILogger logger,
        //    TaskOrchestrationContext context,
        //    string message,
        //    params object[] args)
        //{
        //    if (context == null || logger == null)
        //        return;

        //    if (!context.IsReplaying)
        //    {
        //        logger.LogWarning(message, args);
        //    }
        //}

        /// <summary>
        /// Logs a warning message with an event id if the orchestration is not replaying.
        /// </summary>
        //public static void LogWarning(
        //    this ILogger logger,
        //    TaskOrchestrationContext context,
        //    EventId eventId,
        //    string message,
        //    params object[] args)
        //{
        //    if (context == null || logger == null)
        //        return;

        //    if (!context.IsReplaying)
        //    {
        //        logger.LogWarning(eventId, message, args);
        //    }
        //}
    }
}
