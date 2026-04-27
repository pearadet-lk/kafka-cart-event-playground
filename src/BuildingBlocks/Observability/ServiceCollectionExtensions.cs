using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.CircuitBreaker;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Net;
using Serilog;

namespace BuildingBlocks.Observability;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder, string serviceName)
    {
        builder.Host.UseSerilog((_, loggerConfig) =>
        {
            loggerConfig
                .WriteTo.Console()
                .Enrich.FromLogContext();
        });

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter();
            });
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        return builder;
    }

    public static WebApplication UseDefaultExceptionHandling(this WebApplication app)
    {
        app.UseExceptionHandler();
        return app;
    }

    public static IHttpClientBuilder AddPollyRetryHttpClient(
        this IServiceCollection services,
        string name,
        string baseAddress,
        ResiliencePolicyOptions? options = null)
    {
        var policyOptions = options ?? new ResiliencePolicyOptions();

        return services
            .AddHttpClient(name, client => { client.BaseAddress = new Uri(baseAddress); })
            .AddPolicyHandler(GetRetryPolicy(policyOptions))
            .AddPolicyHandler(GetCircuitBreakerPolicy(policyOptions));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ResiliencePolicyOptions options)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(response => response.StatusCode == HttpStatusCode.NotFound)
            .WaitAndRetryAsync(
                retryCount: options.RetryCount,
                sleepDurationProvider: attempt =>
                    TimeSpan.FromSeconds(Math.Pow(options.BaseDelaySeconds, attempt)));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(ResiliencePolicyOptions options)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: options.CircuitBreakerFailures,
                durationOfBreak: TimeSpan.FromSeconds(options.CircuitBreakerDurationSeconds),
                onBreak: (result, breakDelay) =>
                {
                    Console.WriteLine(
                        $"Circuit opened for {breakDelay.TotalSeconds} seconds due to: {result.Exception?.Message ?? result.Result.StatusCode.ToString()}");
                },
                onReset: () =>
                {
                    Console.WriteLine("Circuit reset. Requests will flow again.");
                },
                onHalfOpen: () =>
                {
                    Console.WriteLine("Circuit half-open. Testing downstream availability.");
                });
    }
}
