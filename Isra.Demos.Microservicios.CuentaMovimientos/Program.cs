using Isra.Demos.Microservicios.CuentaMovimientos;
using Isra.Demos.Microservicios.CuentaMovimientos.Configuracion;
using Isra.Demos.Microservicios.CuentaMovimientos.Repositorio;
using Isra.Demos.Microservicios.Modelo;
using Isra.Demos.Microservicios.Servicios;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Registrar mapeos de MongoDB
MongoDbConfig.RegistrarMapeos();

// Configurar MongoDB
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDb")
                            ?? Constantes.MongoDbConnectionString;

var mongoClient = new MongoClient(mongoConnectionString);
var mongoDatabase = mongoClient.GetDatabase(Constantes.EventStoreDatabaseName);

// Registrar servicios y repositorios
builder.Services.AddSingleton<IMongoClient, MongoClient>();
builder.Services.AddSingleton(mongoDatabase);

builder.Services.AddScoped<IRepositorioEventos, RepositorioEventos>();
builder.Services.AddSingleton<IColaMensajesService, KafkaColaMensajesService>();
builder.Services.AddScoped<ICuentaBancariaService, CuentaBancariaService>();

// agregar servicio background para procesar los mensajes de salida
builder.Services.AddHostedService<ProcesadorMensajesSalidaService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.EnvironmentName == "Development")
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

app.MapControllers();

app.Run();
