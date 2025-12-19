using System;

namespace XRayConnector.Metrics
{
    public enum ApiReplay
    {
        NotSupported,
        Yes,
        No,
    }

    public interface IApiMetricsProvider : IDisposable
    {

        /// <summary>
        /// Increment a counter for REST API. Supply the API- and operation name (e.g. api=XRay and Operation=GetTraceSummaries).
        /// </summary>
        void RecordApiOperation(string api, string account, string operation, bool paged, ApiReplay replay = ApiReplay.NotSupported);

        /// <summary>
        /// Record the number of traces that were processed/retrieved.
        /// </summary>
        /// <param name="count">Number of traces</param>
        void RecordApiResponseObjects(string api, string account, string objectName, long count);

        void RecordMemoryUsage();
     
    }
}