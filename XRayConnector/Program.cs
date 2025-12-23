using Amazon.Runtime;
using Amazon.Runtime.Credentials;
using Amazon.XRay;
using AmazonSDKWrapper;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using System;
using System.Net.Http.Headers;
using XRayConnector;
using XRayConnector.Telemetry;


var builder = FunctionsApplication.CreateBuilder(args);

builder.Services.AddSingleton<WorkflowConfig>();

builder.Services.AddHttpClient("XRayConnector", config =>
{
    var productValue = new ProductInfoHeaderValue("XRayConnector", "1.6");
    var commentValue = new ProductInfoHeaderValue("(+https://github.com/dtPaTh/AWS.XRayExporter)");

    config.DefaultRequestHeaders.UserAgent.Add(productValue);
    config.DefaultRequestHeaders.UserAgent.Add(commentValue);
});


builder.Services.AddSingleton<JsonPayloadHelper>(sp =>
{
    return new JsonPayloadHelper(bool.TryParse(Environment.GetEnvironmentVariable("EnableJsonPayloadCompression"), out bool _result) && _result);
});

builder.Services.AddSingleton<AWSCredentials>(sp =>
{

    AWSCredentials creds = null;
    var identityKey = Environment.GetEnvironmentVariable("AWS_IdentityKey");
    var secretKey = Environment.GetEnvironmentVariable("AWS_SecretKey");
    if (!string.IsNullOrEmpty(identityKey) && !string.IsNullOrEmpty(secretKey))
    {
        creds = new BasicAWSCredentials(identityKey, secretKey);
    }

    var roleArn = Environment.GetEnvironmentVariable("AWS_RoleArn");
    var regionEndpoint = Environment.GetEnvironmentVariable("AWS_RegionEndpoint");

    if (!string.IsNullOrEmpty(roleArn) && !string.IsNullOrEmpty(regionEndpoint))
    {
        // Use AssumeRoleAWSCredentials which refreshes automatically. Provide base credentials for STS calls.

        if (creds == null) // fallback to default credentials chain
            creds = DefaultAWSCredentialsIdentityResolver.GetCredentials();

        return new AssumeRoleAWSCredentials(creds, roleArn, "XRayConnectorRoleSession");
    }

    if (creds != null)
        return creds;
    else // fallback to default credentials chain
        return DefaultAWSCredentialsIdentityResolver.GetCredentials();
});

builder.Services.AddSingleton<IAmazonXRay>(sp =>
{
    var creds = sp.GetRequiredService<AWSCredentials>();
    var regionEndpoint = Environment.GetEnvironmentVariable("AWS_RegionEndpoint");
    if (!string.IsNullOrEmpty(regionEndpoint))
        return new AmazonXRayClient(creds, Amazon.RegionEndpoint.GetBySystemName(regionEndpoint));

    return new AmazonXRayClient(creds);
});

// Choose between simulator or real client based on environment
var simulatorMode = Environment.GetEnvironmentVariable("SimulatorMode");
if (string.Equals(simulatorMode, "XRayApi", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IXRayClient>(_ => new XRayClientSimulator(
        ushort.TryParse(Environment.GetEnvironmentVariable("SIM_TraceSummariesResponseCount"), out var _traceCount) ? _traceCount : (ushort)10,
        byte.TryParse(Environment.GetEnvironmentVariable("SIM_TraceSummariesPageSize"), out var _pageSize) ? _pageSize : (byte)2,
        Enum.TryParse<XRayClientSimulator.BatchSegmentMode>(Environment.GetEnvironmentVariable("SIM_BatchTraceSegments"), out var _batchSegmentMode) ? _batchSegmentMode : XRayClientSimulator.BatchSegmentMode.Never
    ));
}
else
{
    builder.Services.AddSingleton<IXRayClient, XRayClient>();
}

var enableProfiling = Environment.GetEnvironmentVariable("EnableProfiling");
if (bool.TryParse(enableProfiling, out bool profilingEnabled) && profilingEnabled)
{
    builder.Services.AddSingleton<MetricsProvider>();
    var otlpMetricsEndpoint = Environment.GetEnvironmentVariable("OTLP_METRICS_ENDPOINT") ?? "http://localhost:4317";
    builder.Services.AddOpenTelemetryMetrics(metricsBuilder =>
    {
        metricsBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("AWS.XRayExporter"))
            .AddMeter(MetricsProvider.MeterName)
            .AddOtlpExporter(options =>
            {
                // use gRPC protocol to send metrics to OTLP collector
                options.Endpoint = new Uri(otlpMetricsEndpoint);
                options.Protocol = OtlpExportProtocol.Grpc;
            });
    });
}


await builder.Build().RunAsync();



