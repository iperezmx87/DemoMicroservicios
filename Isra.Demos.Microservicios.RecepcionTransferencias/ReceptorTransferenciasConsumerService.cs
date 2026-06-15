using Confluent.Kafka;
using Isra.Demos.Microservicios.RecepcionTransferencias.Modelo;
using Isra.Demos.Microservicios.RecepcionTransferencias.Servicios;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Isra.Demos.Microservicios.RecepcionTransferencias
{
    /// <summary>
    /// Backgrond service que procesa las transferencias enviadas para hacer la aplicación de la misma
    /// </summary>
    public class ReceptorTransferenciasConsumerService
        : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IProducer<string, string> _producer;
        private readonly IConfiguration _configuration;
        private readonly ICuentaBancariaService _cuentaBancariaService;
        private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

        /// <summary>
        /// Constructor del servicio
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="cuentaBancariaService"></param>
        public ReceptorTransferenciasConsumerService(
            IConfiguration configuration,
            ICuentaBancariaService cuentaBancariaService)
        {
            _configuration = configuration;

            _cuentaBancariaService = cuentaBancariaService;

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _configuration.GetValue<string>("Kafka:BootstrapServers"),
                GroupId = "receptor-transferencias-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
            };

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = _configuration.GetValue<string>("Kafka:BootstrapServers")
            };

            _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();

            _producer = new ProducerBuilder<string, string>(producerConfig).Build();
        }

        /// <summary>
        /// Ejecuta la operación
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
                            "Kafka Consume: Recibiendo una transferencia",
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
                                case "TransferenciaRealizadaEvento":
                                    var recepcionTransferencia = JsonSerializer.Deserialize<TransferenciaRecibidaEvento>(eventoJson);

                                    try
                                    {
                                        await _cuentaBancariaService.RecibirTransferenciaAsync(recepcionTransferencia.CuentaDestinoId, recepcionTransferencia.Monto);

                                        _consumer.Commit(result);

                                        Console.WriteLine($"Recepción de transferencia: {recepcionTransferencia.AggregateId} {recepcionTransferencia.Monto}");

                                        activity?.SetStatus(ActivityStatusCode.Ok);
                                    }
                                    catch (InvalidDataException ex)
                                    {
                                        // errores en la validación de la cuenta receptora o del monto
                                        // se tiene que efectuar la devolución del dinero al emisor, para lo cual se puede publicar un nuevo evento de devolución de transferencia
                                        Console.WriteLine("Error al procesar la recepción de transferencia: {0}. Devolviendo el dinero", ex.Message);

                                        activity?.AddEvent(new ActivityEvent("Devolución de transferencia"));
                                        activity?.AddException(ex);

                                        await _cuentaBancariaService.DevolverTransferenciaAsync(
                                           recepcionTransferencia.EventId,
                                           recepcionTransferencia.AggregateId,
                                           ex.Message,
                                           recepcionTransferencia.Monto
                                           );

                                        _consumer.Commit(result);

                                        Console.WriteLine($"Devolución de transferencia: Origen {recepcionTransferencia.AggregateId}; Destino {recepcionTransferencia.CuentaDestinoId} {recepcionTransferencia.Monto}");

                                        activity?.SetStatus(ActivityStatusCode.Error);
                                    }

                                    break;

                                default:
                                    break;
                            }
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

        private async Task EnviarARetryODlqAsync(Message<string, string> message)
        {
            await _producer.ProduceAsync(_configuration.GetValue<string>("Kafka:TopicDlq"), message);
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
        public static ActivitySource Source { get; } = new("Isra.Demos.Microservicios.RecepcionTransferencias");
    }
}
