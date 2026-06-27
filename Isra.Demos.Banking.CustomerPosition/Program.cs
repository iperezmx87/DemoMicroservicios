using Isra.Demos.Banking.CustomerPosition;
using Isra.Demos.Banking.CustomerPosition.Monitoreo;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar OpenTelemetry (Traces, Metrics y Logs)
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(MicroservicioTelemetry.ServiceName))
    .WithTracing(tracing => tracing
        .AddSource(MicroservicioTelemetry.Source.Name) // Escucha nuestros eventos personalizados
        .AddAspNetCoreInstrumentation()          // Rastrea llamadas HTTP entrantes automáticamente
        .AddOtlpExporter(options =>
        {
            // Apunta al puerto gRPC estándar del OpenTelemetry Collector o Jaeger
            options.Endpoint = new Uri(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://localhost:4317");
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()             // Métricas del CLR, GC y ThreadPool de .NET 10
        .AddOtlpExporter());

// 2. Correlacionar los Logs nativos de .NET con OpenTelemetry
builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
    options.AddOtlpExporter();
});

builder.Services.AddHostedService<SaldoConsumerService>();

builder.Services.AddHealthChecks()
    // Check de Kafka
    .AddKafka(
        setup => setup.BootstrapServers = builder.Configuration["Kafka:BootstrapServers"]!,
        name: "Kafka-Broker",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "ready", "broker" }
    )
    .AddNpgSql(
    builder.Configuration.GetConnectionString("PostgresSaldoConnection")!,
    name: "Postgres-Saldo");

var app = builder.Build();

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            duration = report.TotalDuration,
            components = report.Entries.Select(e => new
            {
                key = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        });

        await context.Response.WriteAsync(result);
    }
});

app.Run();
