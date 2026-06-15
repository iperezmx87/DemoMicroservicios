using Confluent.Kafka;
using Dapper;
using Isra.Demos.Microservicios.Saldo.Modelo;
using Npgsql;
using OpenTelemetry.Context.Propagation;
using Polly;
using Polly.Retry;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Isra.Demos.Microservicios.Saldo
{
    /// <summary>
    /// worker que se ejecuta en segundo plano y realiza tareas periódicas, como registrar información sobre su ejecución. En este caso, el worker registra un mensaje cada segundo indicando que está en funcionamiento, lo que puede ser útil para monitorear su estado y asegurarse de que está activo. El uso de un BackgroundService permite que el worker se ejecute de manera asíncrona y se detenga de forma ordenada cuando se solicite la cancelación, lo que es esencial para garantizar la estabilidad y el rendimiento de la aplicación en la que se integra este worker.
    /// </summary>
    public class SaldoConsumerService : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IProducer<string, string> _producer;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly ResiliencePipeline _resiliencePipeline;
        // Usamos el mismo propagador W3C que configuramos en el productor
        private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

        /// <summary>
        /// Constructor
        /// </summary>
        public SaldoConsumerService(IConfiguration configuration)
        {
            _configuration = configuration;

            _connectionString = _configuration["ConnectionStrings:PostgresSaldoConnection"];

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"],
                GroupId = "cuenta-saldo-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"]
            };

            _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();

            _producer = new ProducerBuilder<string, string>(producerConfig).Build();

            _resiliencePipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder().Handle<NpgsqlException>().Handle<TimeoutException>(),
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(2),
                    BackoffType = DelayBackoffType.Exponential,
                    OnRetry = args =>
                    {
                        // Aquí puedes usar ILogger para trazar el reintento
                        Console.WriteLine($"Fallo transitorio en Postgres. Reintento {args.AttemptNumber} debido a: {args.Outcome.Exception?.Message}");
                        return ValueTask.CompletedTask;
                    }
                }).Build();
        }

        /// <summary>
        /// Ejecutar el servicio
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                _consumer.Subscribe(_configuration["Kafka:Topic"]);

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
                            "Kafka Consume: Actualizar Saldo",
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

                                    await ActualizarSaldo(deposito, esDeposito: true);

                                    _consumer.Commit(result);

                                    Console.WriteLine($"Deposito: {deposito.AggregateId} {deposito.Monto}");

                                    break;

                                case "DineroRetiradoEvento":
                                    var retiro = JsonSerializer.Deserialize<DineroRetiradoEvento>(eventoJson);

                                    await ActualizarSaldo(retiro, esDeposito: false);

                                    _consumer.Commit(result);

                                    Console.WriteLine($"Retiro: {retiro.AggregateId} {retiro.Monto}");

                                    break;

                                case "TransferenciaRealizadaEvento":
                                    var transferenciaEnviada = JsonSerializer.Deserialize<TransferenciaRealizadaEvento>(eventoJson);

                                    await ActualizarSaldo(transferenciaEnviada, esDeposito: false);

                                    _consumer.Commit(result);

                                    Console.WriteLine($"Retiro por transferencia: {transferenciaEnviada.AggregateId} {transferenciaEnviada.Monto}");

                                    break;

                                case "TransferenciaRecibidaEvento":
                                    var transferenciaRecibida = JsonSerializer.Deserialize<TransferenciaRecibidaEvento>(eventoJson);

                                    await ActualizarSaldo(transferenciaRecibida, esDeposito: true);

                                    _consumer.Commit(result);

                                    Console.WriteLine($"Deposito por transferencia: {transferenciaRecibida.AggregateId} {transferenciaRecibida.Monto}");

                                    break;

                                case "TransferenciaDevueltaEvento":
                                    var transferenciaDevuelta = JsonSerializer.Deserialize<TransferenciaDevueltaEvento>(eventoJson);

                                    await ActualizarSaldo(transferenciaDevuelta, esDeposito: true);

                                    _consumer.Commit(result);

                                    Console.WriteLine($"Deposito por devolución: {transferenciaDevuelta.AggregateId} {transferenciaDevuelta.Monto}");

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
                    catch (OperationCanceledException) { break; }
                    catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
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

        private async Task ActualizarSaldo(EventoBase evento, bool esDeposito)
        {
            await _resiliencePipeline.ExecuteAsync(async token =>
            {
                using var conn = new NpgsqlConnection(_connectionString);

                await conn.OpenAsync();

                // El monto depende de si es deposito o retiro
                decimal montoModificador = 0;

                if (evento is DineroDepositadoEvento d) { montoModificador = d.Monto; }
                if (evento is DineroRetiradoEvento r) { montoModificador = -r.Monto; }
                if (evento is TransferenciaRealizadaEvento t) { montoModificador = -t.Monto; }
                if (evento is TransferenciaDevueltaEvento td) { montoModificador = td.Monto; }
                if (evento is TransferenciaRecibidaEvento tr) { montoModificador = tr.Monto; }

                // SQL Upsert con validación de versión
                // Solo actualiza si la versión del evento es mayor a la que tenemos
                string sql = @"
            INSERT INTO cuentas.saldos_cuenta (id, saldo, ultima_version)
            VALUES (@Id, @Monto, @Version)
            ON CONFLICT (id) DO UPDATE 
            SET saldo = cuentas.saldos_cuenta.saldo + EXCLUDED.saldo,
                ultima_version = EXCLUDED.ultima_version,
                ultima_actualizacion = CURRENT_TIMESTAMP
            WHERE cuentas.saldos_cuenta.ultima_version < EXCLUDED.ultima_version;";

                await conn.ExecuteAsync(sql, new
                {
                    Id = evento.AggregateId,
                    Monto = montoModificador,
                    evento.Version
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
        public static ActivitySource Source { get; } = new("Isra.Demos.Microservicios.Saldo");
    }
}
