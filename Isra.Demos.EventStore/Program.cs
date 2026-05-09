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

builder.Services.AddSingleton(mongoDatabase);
builder.Services.AddScoped<IRepositorioEventos, RepositorioEventos>();
builder.Services.AddScoped<IColaMensajesService, KafkaColaMensajesService>();
builder.Services.AddScoped<ICuentaBancariaService, CuentaBancariaService>();

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
