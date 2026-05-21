using Confluent.Kafka;
using Isra.Demos.Microservicios.RecepcionTransferencias.Modelo;
using Isra.Demos.Microservicios.RecepcionTransferencias.Servicios;
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
        private readonly IConfiguration _configuration;
        private readonly ICuentaBancariaService _cuentaBancariaService;

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

            var config = new ConsumerConfig
            {
                BootstrapServers = _configuration.GetValue<string>("Kafka:BootstrapServers"),
                GroupId = "receptor-transferencias-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();
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

                                await _cuentaBancariaService.RecibirTransferenciaAsync(recepcionTransferencia.CuentaDestinoId, recepcionTransferencia.Monto);

                                Console.WriteLine($"Recepción de transferencia: {recepcionTransferencia.AggregateId} {recepcionTransferencia.Monto}");

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
    }
}
