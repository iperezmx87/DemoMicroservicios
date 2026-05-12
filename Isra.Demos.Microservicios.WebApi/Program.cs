using Isra.Demos.Microservicios.WebApi.Contratos;
using Isra.Demos.Microservicios.WebApi.Repositorio;
using Isra.Demos.Microservicios.WebApi.Servicios;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// agregar repositorios
builder.Services.AddScoped<IEstadoCuentaRepositorio, EstadoCuentaRepositorio>();
builder.Services.AddScoped<ISaldoRepositorio, SaldoRepositorio>();
builder.Services.AddScoped<IGeneradorEstadoCuentaPdfService, GeneradorEstadoCuentaPdfService>();

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
