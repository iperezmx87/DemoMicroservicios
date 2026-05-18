using Isra.Demos.Microservicios.UsuariosCuentas.Repositorio;
using Isra.Demos.Microservicios.UsuariosCuentas.Servicio;

var builder = WebApplication.CreateBuilder(args);

// Configuración de CORS para permitir al frontend conectarse
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendReact", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:4200", "http://localhost:5116")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add services to the container.
builder.Services.AddScoped<ICuentaServicio, CuentaServicio>();
builder.Services.AddScoped<ICuentaRepositorio, CuentaRepositorio>();


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

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
