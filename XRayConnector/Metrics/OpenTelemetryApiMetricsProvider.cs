using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace XRayConnector.Metrics
{
    public sealed class OpenTelemetryApiMetricsProvider : IApiMetricsProvider
    {
        public const string MeterName = "AWS.XRayExporter";
        public const string MeterVersion = "1.0.0";

    
        private readonly Meter _meter;
        private readonly Counter<long> _apiOperations;
        private readonly Counter<long> _apiResponseObjects;
        private readonly Gauge<long> _processMemoryUsageTotal;
        private readonly Gauge<long> _processMemoryUsageManaged;
        private bool _disposed;

        public OpenTelemetryApiMetricsProvider()
        {
            _meter = new Meter(MeterName, MeterVersion);
            _apiOperations = _meter.CreateCounter<long>("api_calls", description: "Number of api requests");
            _apiResponseObjects = _meter.CreateCounter<long>("api_response_objects", description: "Number of objects returned");
            _processMemoryUsageTotal = _meter.CreateGauge<long>("process_memory_usage.total", description: "Process total memory usage in bytes");
            _processMemoryUsageManaged = _meter.CreateGauge<long>("process_memory_usage.gc_heap", description: "Process managed memory usage in bytes");
        }

        public void RecordApiOperation(string api, string account, string operation, bool paged, ApiReplay replay = ApiReplay.NotSupported)
        {
            if (replay == ApiReplay.NotSupported)
                _apiOperations.Add(1, [new("api", api), new("account", account), new("operation", operation), new("paged", paged ? "yes" : "no")]);
            else
                _apiOperations.Add(1, [new("api", api), new("account", account), new("operation", operation), new("paged", paged ? "yes" : "no"), new("replay", replay == ApiReplay.Yes ? "yes" : "no")]);
        }

        public void RecordApiResponseObjects(string api, string account, string objectName, long count)
        {
            _apiResponseObjects.Add(count, [new("api", api), new("account", account), new("objectName", objectName)]);
        }

        public void RecordMemoryUsage()
        {
            _processMemoryUsageTotal.Record(Process.GetCurrentProcess().WorkingSet64);
            _processMemoryUsageManaged.Record(GC.GetTotalMemory(false));
            
        }

         
        public void Dispose()
        {
            if (_disposed) return;
            _meter?.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    
    }
}