using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.Extensions;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http.Headers;
using XRayConnector;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Services.AddHttpClient("XRayConnector", config =>
{
    var productValue = new ProductInfoHeaderValue("XRayConnector", "1.0");
    var commentValue = new ProductInfoHeaderValue("(+https://github.com/dtPaTh/AWS.XRayExporter)");

    config.DefaultRequestHeaders.UserAgent.Add(productValue);
    config.DefaultRequestHeaders.UserAgent.Add(commentValue);
});


builder.Services.AddSingleton<JsonPayloadHelper>(sp =>
{
    return new JsonPayloadHelper(bool.TryParse(Environment.GetEnvironmentVariable("EnableJsonPayloadCompression"), out bool _result) && _result);
});



builder.Build().Run();


