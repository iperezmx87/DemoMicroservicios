using Isra.Demos.Microservicios.RecepcionTransferencias;
using Isra.Demos.Microservicios.RecepcionTransferencias.Configuracion;
using Isra.Demos.Microservicios.RecepcionTransferencias.Consultas;
using Isra.Demos.Microservicios.RecepcionTransferencias.Repositorio;
using Isra.Demos.Microservicios.RecepcionTransferencias.Servicios;
using MongoDB.Driver;


var builder = Host.CreateApplicationBuilder(args);

// Registrar mapeos de MongoDB
MongoDbConfig.RegistrarMapeos();

// Configurar MongoDB
var mongoConnectionString = builder.Configuration.GetValue("MongoDB:ConnectionString", "mongodb://localhost:27017");

var mongoClient = new MongoClient(mongoConnectionString);
var mongoDatabase = mongoClient.GetDatabase(builder.Configuration.GetValue("MongoDB:DatabaseName", "bd_cuentas_movimientos"));

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
