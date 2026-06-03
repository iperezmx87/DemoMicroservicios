using Isra.Demos.Microservicios.CuentaMovimientos;
using Isra.Demos.Microservicios.CuentaMovimientos.Configuracion;
using Isra.Demos.Microservicios.CuentaMovimientos.Consultas;
using Isra.Demos.Microservicios.CuentaMovimientos.Repositorio;
using Isra.Demos.Microservicios.CuentaMovimientos.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

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
