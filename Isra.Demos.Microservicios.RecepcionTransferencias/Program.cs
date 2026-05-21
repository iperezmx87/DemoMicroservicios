using Isra.Demos.Microservicios.RecepcionTransferencias;
using Isra.Demos.Microservicios.RecepcionTransferencias.Configuracion;
using MongoDB.Driver;


var builder = Host.CreateApplicationBuilder(args);

// Registrar mapeos de MongoDB
MongoDbConfig.RegistrarMapeos();

// Configurar MongoDB
var mongoConnectionString = builder.Configuration.GetValue("MongoDB:ConnectionString", "mongodb://localhost:27017");

var mongoClient = new MongoClient(mongoConnectionString);
var mongoDatabase = mongoClient.GetDatabase(builder.Configuration.GetValue("MongoDB:DatabaseName", "bd_cuentas_movimientos"));

// Registrar servicios y repositorios
builder.Services.AddSingleton<IMongoClient, MongoClient>();
builder.Services.AddSingleton(mongoDatabase);

builder.Services.AddHostedService<>();

var host = builder.Build();
host.Run();
