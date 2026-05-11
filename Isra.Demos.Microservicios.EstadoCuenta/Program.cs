using Isra.Demos.Microservicios.EstadoCuenta;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<EstadoCuentaConsumerService>();

var host = builder.Build();
host.Run();
