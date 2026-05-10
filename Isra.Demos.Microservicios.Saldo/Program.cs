using Isra.Demos.Microservicios.Saldo;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<SaldoConsumerService>();

var host = builder.Build();
host.Run();
