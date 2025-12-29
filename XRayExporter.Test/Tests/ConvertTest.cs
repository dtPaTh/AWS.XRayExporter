using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json.Linq;
using Opentelemetry.Proto.Collector.Trace.V1;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Opentelemetry.Proto.Trace.V1;

namespace XRayExporter.Test
{
    public class ConvertTest
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void ConvertFromSample()
        {
            var tracesJson = File.ReadAllText("data/batchgettraces.json");
            var conv = new XRay2OTLP.Convert(null);
            var otlp = conv.FromXRay(tracesJson);

            Assert.That(otlp, Is.Not.Null);
            Assert.That(otlp.ResourceSpans, Is.Not.Null);

            // Expect one ResourceSpans entry per top-level segment in the sample (5 segments)
            Assert.That(otlp.ResourceSpans.Count, Is.EqualTo(5));

            // collect all spans
            var spans = otlp.ResourceSpans
                .SelectMany(rs => rs.InstrumentationLibrarySpans)
                .SelectMany(lib => lib.Spans)
                .ToList();

            Assert.That(spans.Count, Is.GreaterThanOrEqualTo(5));

            // The Scorekeep segment contains an HTTP POST to /api/user -> span name should start with POST and contain the path
            Assert.That(spans.Any(s => s.Name != null && s.Name.StartsWith("POST") && s.Name.Contains("/api/user")), Is.True, "Expected a span with HTTP POST to /api/user");

            // There should be a dateabase span
            Assert.That(spans.Any(s => s.Name != null && s.Name.StartsWith("UpdateItem") && s.Name.Contains("scorekeep-user") && s.Attributes.Any(a=> a.Key == "db.system" && a.Value.StringValue == "dynamodb"  )), Is.True, "Expected a span with UpdateItem scorekeep-user with db.system =dynamodb");

            // At least one span should have an HTTP 200 mapped to Status.Unset
            Assert.That(spans.Any(s => s.Status != null && s.Status.Code == Status.Types.StatusCode.Unset), Is.True, "Expected at least one span with Status.Unset for HTTP 200 responses");

            // All spans should include X-Ray trace/context attributes (trace id and segment id)
            Assert.That(spans.Any(s => s.Attributes.Any(a => a.Key == "aws.xray.trace_id" && a.Value.StringValue != String.Empty)), Is.True, "Expected at least one span with aws.xray.trace_id attribute");
            Assert.That(spans.Any(s => s.Attributes.Any(a => a.Key == "aws.xray.segment_id" && a.Value.StringValue != String.Empty)), Is.True, "Expected at least one span with aws.xray.segment_id attribute");

            // At least one span should have a ParentSpanId (child span)
            Assert.That(spans.Any(s => s.ParentSpanId != null && s.ParentSpanId.Length > 0), Is.True, "Expected at least one child span with ParentSpanId");

            // Resource-level attributes should include cloud.provider = aws
            Assert.That(otlp.ResourceSpans.Any(rs => rs.Resource != null && rs.Resource.Attributes.Any(a => a.Key == "cloud.provider" && a.Value.StringValue == "aws")), Is.True, "Expected resource with cloud.provider=aws");

            // Instrumentation library name should include X-Ray for at least one library
            Assert.That(otlp.ResourceSpans.SelectMany(rs => rs.InstrumentationLibrarySpans).Any(lib => lib.InstrumentationLibrary != null && (lib.InstrumentationLibrary.Name?.Contains("X-Ray") == true || lib.InstrumentationLibrary.Name?.Contains("XRay") == true)), Is.True, "Expected instrumentation library name containing 'X-Ray' or 'XRay'");
        }

    }
}