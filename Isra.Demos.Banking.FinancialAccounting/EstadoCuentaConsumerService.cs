using Confluent.Kafka;
using Dapper;
using Isra.Demos.Banking.FinancialAccounting.Modelo;
using Microsoft.Data.SqlClient;
using OpenTelemetry.Context.Propagation;
using Polly;
using Polly.Retry;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Isra.Demos.Banking.FinancialAccounting
{
    /// <summary>
    /// Servicio generador de eventos de estado de cuenta, que se suscribe a un tópico de Kafka y actualiza el estado de cuenta en una base de datos SQL Server.
    /// </summary>
    public class EstadoCuentaConsumerService : BackgroundService
    {
        private readonly string _connectionString;
        private readonly IConsumer<string, string> _consumer;
        private readonly IProducer<string, string> _producer;
        private readonly IConfiguration _configuration;
        private readonly ResiliencePipeline _resiliencePipeline;
        // Usamos el mismo propagador W3C que configuramos en el productor
        private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

        /// <summary>
        /// Constructor del servicio, que inicializa la conexión a la base de datos y el consumidor de Kafka.
        /// </summary>
        public EstadoCuentaConsumerService(IConfiguration configuration)
        {
            _configuration = configuration;

            _connectionString = _configuration.GetValue<string>("ConnectionStrings:SQLServerEstadoCuentaConnectionString");

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _configuration.GetValue<string>("Kafka:BootstrapServers"),
                GroupId = "cuenta-estado-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = _configuration.GetValue<string>("Kafka:BootstrapServers")
            };

            _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();

            _producer = new ProducerBuilder<string, string>(producerConfig).Build();

            _resiliencePipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder().Handle<SqlException>().Handle<TimeoutException>(),
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(2),
                    BackoffType = DelayBackoffType.Exponential,
                    OnRetry = args =>
                    {
                        // Aquí puedes usar ILogger para trazar el reintento
                        Console.WriteLine($"Fallo transitorio en SQL Server. Reintento {args.AttemptNumber} debido a: {args.Outcome.Exception?.Message}");
                        return ValueTask.CompletedTask;
                    }
                }).Build();
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

                        if (result is null)
                            continue;

                        // 1. EXTRAER EL CONTEXTO: Leemos los headers binarios de Kafka
                        var parentContext = Propagator.Extract(default, result.Message.Headers, ExtractTraceContext);

                        // 2. INICIAR ACTIVIDAD HIJA: Arrancamos un Span tipo 'Consumer' amarrado al TraceId original
                        // Pasamos 'parentContext.ActivityContext' para que herede la identidad exacta
                        using Activity activity = SaldoTelemetry.Source.StartActivity(
                            "Kafka Consume: Actualizando el estado de cuenta",
                            ActivityKind.Consumer,
                            parentContext.ActivityContext);

                        if (activity != null)
                        {
                            activity.SetTag("messaging.system", "kafka");
                            activity.SetTag("messaging.destination", result.Topic);
                            activity.SetTag("messaging.kafka.key", result.Message.Key);
                        }

                        try
                        {
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

                                    _consumer.Commit(result);

                                    Console.WriteLine($"Deposito: {deposito.AggregateId} {deposito.Monto}");
                                    break;

                                case "DineroRetiradoEvento":
                                    var retiro = JsonSerializer.Deserialize<DineroRetiradoEvento>(eventoJson);

                                    await RegistrarMovimiento(retiro, "Retiro");

                                    _consumer.Commit(result);

                                    Console.WriteLine($"Retiro: {retiro.AggregateId} {retiro.Monto}");
                                    break;

                                case "TransferenciaRealizadaEvento":
                                    var envioTransferencia = JsonSerializer.Deserialize<TransferenciaRealizadaEvento>(eventoJson);

                                    await RegistrarMovimiento(envioTransferencia, "Envío de dinero transferencia");

                                    _consumer.Commit(result);

                                    Console.WriteLine($"Envío de transferencia: {envioTransferencia.AggregateId} {envioTransferencia.Monto}");
                                    break;

                                case "TransferenciaRecibidaEvento":
                                    var recepcionTransferencia = JsonSerializer.Deserialize<TransferenciaRecibidaEvento>(eventoJson);

                                    await RegistrarMovimiento(recepcionTransferencia, "Recepción de dinero transferencia");

                                    _consumer.Commit(result);

                                    Console.WriteLine($"Recepción de transferencia: {recepcionTransferencia.AggregateId} {recepcionTransferencia.Monto}");

                                    break;

                                case "TransferenciaDevueltaEvento":
                                    var devolucionTransferencia = JsonSerializer.Deserialize<TransferenciaDevueltaEvento>(eventoJson);

                                    await RegistrarMovimiento(devolucionTransferencia, "Devolución de dinero transferencia");

                                    _consumer.Commit(result);

                                    Console.WriteLine($"Devolución de transferencia: {devolucionTransferencia.AggregateId} {devolucionTransferencia.Monto}");

                                    break;

                                default:
                                    break;
                            }

                            activity?.SetStatus(ActivityStatusCode.Ok);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error crítico procesando el evento {result.Message.Key}. Enviando a Retry/DLQ.");

                            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                            activity?.AddException(ex);

                            await EnviarARetryODlqAsync(result.Message);

                            _consumer.Commit(result);
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

        private async Task EnviarARetryODlqAsync(Message<string, string> message)
        {
            await _producer.ProduceAsync(_configuration.GetValue<string>("Kafka:TopicDlq"), message);
        }

        /// <summary>
        /// Función auxiliar que OpenTelemetry usa para leer los datos del Header de Confluent.Kafka
        /// </summary>
        private IEnumerable<string> ExtractTraceContext(Headers headers, string key)
        {
            if (headers.TryGetLastBytes(key, out var bytes))
            {
                return [Encoding.UTF8.GetString(bytes)];
            }
            return Enumerable.Empty<string>();
        }

        private async Task RegistrarMovimiento(dynamic evento, string tipo)
        {
            await _resiliencePipeline.ExecuteAsync(async token =>
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
            });
        }
    }

    /// <summary>
    /// Clase de telemetría segura aislada para el microservicio de Saldo
    /// </summary>
    public static class SaldoTelemetry
    {
        /// <summary>
        /// Fuente del trace
        /// </summary>
        public static ActivitySource Source { get; } = new("Isra.Demos.Microservicios.EstadoCuenta");
    }
}
