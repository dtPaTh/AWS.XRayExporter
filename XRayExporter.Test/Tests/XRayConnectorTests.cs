using NUnit.Framework;
using System;
using Microsoft.Extensions.Logging.Abstractions;
using XRayConnector;
using System.Threading.Tasks;
using AmazonSDKWrapper;
using System.Net.Http;
using System.IO;
using System.Text.Json;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;

namespace XRayExporter.Test
{
    class TestHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;
        public TestHttpClientFactory(HttpMessageHandler handler)
        {
            _handler = handler;
        }
        public HttpClient CreateClient(string name)
        {
            return new HttpClient(_handler, disposeHandler: false);
        }
    }

    class AlwaysOkHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var msg = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            msg.Content = new StringContent("{}", Encoding.UTF8, "application/json");
            return Task.FromResult(msg);
        }
    }

    [TestFixture]
    public class XRayConnectorTests
    {
        private XRayConnector.XRayConnector _sut;
        private JsonPayloadHelper _jsonHelper;
        private IXRayClient _sim;
        private IHttpClientFactory _httpFactory;

        [SetUp]
        public void Setup()
        {
            _jsonHelper = new JsonPayloadHelper(true);
            _sim = new XRayClientSimulator();
            _httpFactory = new TestHttpClientFactory(new AlwaysOkHandler());

            var config = new WorkflowConfig(NullLogger<WorkflowConfig>.Instance);
            _sut = new XRayConnector.XRayConnector(config, NullLogger<XRayConnector.XRayConnector>.Instance, _httpFactory, _jsonHelper, _sim, null);

            Environment.SetEnvironmentVariable("OTLP_ENDPOINT", "http://localhost/v1/traces");
            Environment.SetEnvironmentVariable("OTLP_HEADER_AUTHORIZATION", null);
        }

        [Test]
        public async Task GetTraces_Returns_TracesResult_When_Summaries_Present()
        {
            var req = new Amazon.XRay.Model.GetTraceSummariesRequest();
            var res = await _sut.GetTraces(req);
            Assert.That(res, Is.Not.Null);
            Assert.That(res.TraceIds, Is.Not.Null);
            using(var e = res.TraceIds.GetEnumerator())
            {
                Assert.That(e.MoveNext(), Is.True);
                Assert.That(e.Current.Length, Is.EqualTo(2));
            }
        }

        [Test]
        public async Task GetRecentTraceIds_Delegates_To_GetTraces()
        {
            var tr = new TracesRequest { StartTime = DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(10)), EndTime = DateTime.UtcNow };
            var res = await _sut.GetRecentTraceIds(tr);
            Assert.That(res, Is.Not.Null);
        }

        [Test]
        public async Task GetTraceDetails_Returns_Compressed_Traces()
        {
            var activityReq = new TraceDetailsRequest { TraceIds = new string[] { "1-abc" } };
            var details = await _sut.GetTraceDetails(activityReq);

            Assert.That(details, Is.Not.Null);
            Assert.That(details.Traces, Is.Not.Null);
            var decompressed = _jsonHelper.Deserialize(details.Traces);
            Assert.That(decompressed.StartsWith("["), Is.True);
        }

        [Test]
        public async Task ProcessTraces_With_Simulator_Returns_True()
        {
            var activityReq = new TraceDetailsRequest { TraceIds = new string[] { "1-abc" } };
            var details = await _sut.GetTraceDetails(activityReq);

            var success = await _sut.ProcessTraces(details.Traces);
            Assert.That(success, Is.True);
        }

        [Test]
        public void WorkflowWatchdog_InvokeWithNulls_TaskThrowsOnAwait()
        {
            var mi = typeof(XRayConnector.XRayConnector).GetMethod("WorkflowWatchdog", BindingFlags.Public | BindingFlags.Instance);
            var taskObj = mi.Invoke(_sut, new object[] { null, null }) as Task;
            Assert.That(taskObj, Is.Not.Null);
            Assert.ThrowsAsync<NullReferenceException>(async () => await taskObj);
        }

        [Test]
        public void TriggerPeriodicAPIPoller_InvokeWithNulls_TaskThrowsOnAwait()
        {
            var mi = typeof(XRayConnector.XRayConnector).GetMethod("TriggerPeriodicAPIPoller", BindingFlags.Public | BindingFlags.Instance);
            var taskObj = mi.Invoke(_sut, new object[] { null, null }) as Task;
            Assert.That(taskObj, Is.Not.Null);
            Assert.ThrowsAsync<NullReferenceException>(async () => await taskObj);
        }
    }
}
