using Isra.Demos.Microservicios.CuentaMovimientos;
using Isra.Demos.Microservicios.CuentaMovimientos.Configuracion;
using Isra.Demos.Microservicios.CuentaMovimientos.Consultas;
using Isra.Demos.Microservicios.CuentaMovimientos.Infrastructure;
using Isra.Demos.Microservicios.CuentaMovimientos.Repositorio;
using Isra.Demos.Microservicios.CuentaMovimientos.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Polly.Retry;
using System.Diagnostics;
using System.Text;

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

// Configuración de CORS para permitir al frontend conectarse
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// polly
var resiliencePipeline = new ResiliencePipelineBuilder()
    .AddRetry(new RetryStrategyOptions
    {
        ShouldHandle = new PredicateBuilder().Handle<SqlException>().Handle<TimeoutException>(),
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(2),
        BackoffType = DelayBackoffType.Exponential,
        OnRetry = args =>
        {
            // Aquí puedes usar ILogger para trazar el reintento
            Console.WriteLine($"Fallo transitorio en MongoDB. Reintento {args.AttemptNumber} debido a: {args.Outcome.Exception?.Message}");
            return ValueTask.CompletedTask;
        }
    }).Build();

builder.Services.AddSingleton(resiliencePipeline);

// Configuración de MongoDB para ignorar campos adicionales que no estén mapeados en las clases de eventos. Esto es útil para evitar errores de deserialización si se agregan nuevos campos a los eventos en el futuro.
var conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);

// Registrar mapeos de MongoDB
MongoDbConfig.RegistrarMapeos();

// Configurar MongoDB
var mongoConnectionString = builder.Configuration.GetValue("MongoDB:ConnectionString", "mongodb://localhost:27017");

var mongoClient = new MongoClient(mongoConnectionString);
var mongoDatabase = mongoClient.GetDatabase(builder.Configuration.GetValue("MongoDB:DatabaseName", "bd_cuentas_movimientos"));

// Registrar servicios y repositorios
builder.Services.AddSingleton<IMongoClient>(mongoClient);
builder.Services.AddSingleton(mongoDatabase);

builder.Services.AddScoped<IRepositorioEventos, RepositorioEventos>();
builder.Services.AddSingleton<IColaMensajesService, KafkaColaMensajesService>();
builder.Services.AddScoped<ICuentaBancariaService, CuentaBancariaService>();
builder.Services.AddScoped<ObtenerCuentaPorIdConsulta>();

// agregar servicio background para procesar los mensajes de salida
builder.Services.AddHostedService<ProcesadorMensajesSalidaService>();

// healthckeck
builder.Services.AddHealthChecks()
    .AddKafka(
        setup => setup.BootstrapServers = builder.Configuration.GetValue<string>("Kafka:BootstrapServers")!,
        name: "kafka",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
        tags: new[] { "ready", "broker" })
    .AddMongoDb(
        sp => sp.GetRequiredService<IMongoClient>(),
        name: "MongoDB-EventStore",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
        tags: new[] { "ready", "db" }
    );

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

// Configure the HTTP request pipeline.
if (app.Environment.EnvironmentName == "Development")
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
