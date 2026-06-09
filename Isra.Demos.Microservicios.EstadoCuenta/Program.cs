using Isra.Demos.Microservicios.EstadoCuenta;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder();

builder.Services.AddHostedService<EstadoCuentaConsumerService>();

builder.Services.AddHealthChecks()
    // Check de Kafka
    .AddKafka(
        setup => setup.BootstrapServers = builder.Configuration["Kafka:BootstrapServers"]!,
        name: "Kafka-Broker",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "ready", "broker" }
    )
    .AddSqlServer(
    builder.Configuration.GetConnectionString("SQLServerEstadoCuentaConnectionString")!,
    name: "SQL Server - Cuentas");

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
