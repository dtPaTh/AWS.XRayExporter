using System;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Metrics;

namespace XRayConnector.Metrics
{
    /// <summary>
    /// Service collection helpers for OpenTelemetry metrics integration.
    /// </summary>
    public static class OpenTelemetryServiceCollectionExtensions
    {
        /// <summary>
        /// Builds a <see cref="MeterProvider"/> using the provided configuration callback and registers it as a singleton.
        /// Call from Program.cs: builder.Services.AddOpenTelemetryMetrics(b => { ... });
        /// </summary>
        public static IServiceCollection AddOpenTelemetryMetrics(this IServiceCollection services, Action<MeterProviderBuilder> configure)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));
            if (configure is null) throw new ArgumentNullException(nameof(configure));

            // Create and configure the MeterProvider builder, then build the provider.
            var builder = Sdk.CreateMeterProviderBuilder();
            configure(builder);
            var meterProvider = builder.Build();

            // Register the built MeterProvider so exporters/startup code can rely on DI to resolve it.
            services.AddSingleton<MeterProvider>(meterProvider);

            return services;
        }
    }
}