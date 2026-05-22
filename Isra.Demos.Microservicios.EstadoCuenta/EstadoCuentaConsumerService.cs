using Confluent.Kafka;
using Dapper;
using Isra.Demos.Microservicios.EstadoCuenta.Modelo;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace Isra.Demos.Microservicios.EstadoCuenta
{
    /// <summary>
    /// Servicio generador de eventos de estado de cuenta, que se suscribe a un tópico de Kafka y actualiza el estado de cuenta en una base de datos SQL Server.
    /// </summary>
    public class EstadoCuentaConsumerService : BackgroundService
    {
        private readonly string _connectionString;
        private readonly IConsumer<string, string> _consumer;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor del servicio, que inicializa la conexión a la base de datos y el consumidor de Kafka.
        /// </summary>
        public EstadoCuentaConsumerService(IConfiguration configuration)
        {
            _configuration = configuration;

            _connectionString = _configuration.GetValue<string>("ConnectionStrings:SQLServerEstadoCuentaConnectionString");

            var config = new ConsumerConfig
            {
                BootstrapServers = _configuration.GetValue<string>("Kafka:BootstrapServers"),
                GroupId = "cuenta-estado-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();
        }

        /// <summary>
        /// Ejecuta la instancia del servicio, suscribiéndose al tópico de Kafka y procesando los mensajes recibidos para actualizar el estado de cuenta en la base de datos.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                _consumer.Subscribe(_configuration.GetValue<string>("Kafka:Topic"));

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = _consumer.Consume(stoppingToken);
                        var eventoJson = result.Message.Value;

                        // 1. Analizamos el JSON sin deserializarlo a clase todavía
                        using JsonDocument doc = JsonDocument.Parse(eventoJson);
                        JsonElement root = doc.RootElement;

                        // 2. Buscamos la propiedad que diferencia el evento (ej. "TipoEvento")
                        string tipoEvento = root.GetProperty("TipoEvento").GetString();

                        switch (tipoEvento)
                        {
                            case "DineroDepositadoEvento":
                                var deposito = JsonSerializer.Deserialize<DineroDepositadoEvento>(eventoJson);

                                await RegistrarMovimiento(deposito, "Deposito");

                                Console.WriteLine($"Deposito: {deposito.AggregateId} {deposito.Monto}");
                                break;

                            case "DineroRetiradoEvento":
                                var retiro = JsonSerializer.Deserialize<DineroRetiradoEvento>(eventoJson);

                                await RegistrarMovimiento(retiro, "Retiro");

                                Console.WriteLine($"Retiro: {retiro.AggregateId} {retiro.Monto}");
                                break;

                            case "TransferenciaRealizadaEvento":
                                var envioTransferencia = JsonSerializer.Deserialize<TransferenciaRealizadaEvento>(eventoJson);

                                await RegistrarMovimiento(envioTransferencia, "Envío de dinero transferencia");

                                Console.WriteLine($"Envío de transferencia: {envioTransferencia.AggregateId} {envioTransferencia.Monto}");
                                break;

                            case "TransferenciaRecibidaEvento":
                                var recepcionTransferencia = JsonSerializer.Deserialize<TransferenciaRecibidaEvento>(eventoJson);

                                await RegistrarMovimiento(recepcionTransferencia, "Recepción de dinero transferencia");

                                Console.WriteLine($"Recepción de transferencia: {recepcionTransferencia.AggregateId} {recepcionTransferencia.Monto}");

                                break;

                            case "TransferenciaDevueltaEvento":
                                var devolucionTransferencia = JsonSerializer.Deserialize<TransferenciaDevueltaEvento>(eventoJson);

                                await RegistrarMovimiento(devolucionTransferencia, "Devolución de dinero transferencia");

                                Console.WriteLine($"Devolución de transferencia: {devolucionTransferencia.AggregateId} {devolucionTransferencia.Monto}");

                                break;

                            default:
                                break;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // El servicio se está deteniendo, salir del bucle
                        break;
                    }
                    catch (Exception ex)
                    {
                        // Manejar otras excepciones según sea necesario
                        Console.WriteLine($"Error al consumir mensaje: {ex.Message}");
                    }
                }
            }, stoppingToken);
        }

        private async Task RegistrarMovimiento(dynamic evento, string tipo)
        {
            using var conn = new SqlConnection(_connectionString);

            await conn.OpenAsync();

            // SQL Idempotente para SQL Server
            string sql = @"
            IF NOT EXISTS (SELECT 1 FROM MovimientosCuenta WHERE AggregateId = @AggregateId AND Version = @Version)
            BEGIN
                INSERT INTO MovimientosCuenta (AggregateId, TipoMovimiento, Monto, Version, MotivoDevolucion)
                VALUES (@AggregateId, @Tipo, @Monto, @Version, @MotivoDevolucion)
            END";

            await conn.ExecuteAsync(sql, new
            {
                evento.AggregateId,
                Tipo = tipo,
                evento.Monto,
                evento.Version,
                evento.MotivoDevolucion
            });
        }
    }
}
