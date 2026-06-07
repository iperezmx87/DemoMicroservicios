using Isra.Demos.Microservicios.RecepcionTransferencias;
using Isra.Demos.Microservicios.RecepcionTransferencias.Configuracion;
using Isra.Demos.Microservicios.RecepcionTransferencias.Consultas;
using Isra.Demos.Microservicios.RecepcionTransferencias.Repositorio;
using Isra.Demos.Microservicios.RecepcionTransferencias.Servicios;
using Microsoft.Data.SqlClient;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Polly;
using Polly.Retry;

var builder = Host.CreateApplicationBuilder(args);

var conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);

// Registrar mapeos de MongoDB
MongoDbConfig.RegistrarMapeos();

// Configurar MongoDB
var mongoConnectionString = builder.Configuration.GetValue("MongoDB:ConnectionString", "mongodb://localhost:27017");

var mongoClient = new MongoClient(mongoConnectionString);
var mongoDatabase = mongoClient.GetDatabase(builder.Configuration.GetValue("MongoDB:DatabaseName", "bd_cuentas_movimientos"));

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

// Registrar servicios y repositorios
builder.Services.AddSingleton<IMongoClient>(mongoClient);
builder.Services.AddSingleton(mongoDatabase);
builder.Services.AddSingleton<ObtenerCuentaPorIdConsulta>();
builder.Services.AddSingleton<IColaMensajesService, KafkaColaMensajesService>();
builder.Services.AddSingleton<IRepositorioEventos, RepositorioEventos>();
builder.Services.AddSingleton<ICuentaBancariaService, CuentaBancariaService>();

builder.Services.AddHostedService<ProcesadorMensajesSalidaService>();
builder.Services.AddHostedService<ReceptorTransferenciasConsumerService>();

var host = builder.Build();
host.Run();
