using Confluent.Kafka;
using Isra.Demos.EventSource.Models;
using System.Text.Json;

namespace Isra.Demos.Microservicios.Saldo
{
    /// <summary>
    /// worker que se ejecuta en segundo plano y realiza tareas periódicas, como registrar información sobre su ejecución. En este caso, el worker registra un mensaje cada segundo indicando que está en funcionamiento, lo que puede ser útil para monitorear su estado y asegurarse de que está activo. El uso de un BackgroundService permite que el worker se ejecute de manera asíncrona y se detenga de forma ordenada cuando se solicite la cancelación, lo que es esencial para garantizar la estabilidad y el rendimiento de la aplicación en la que se integra este worker.
    /// </summary>
    public class Worker : BackgroundService
    {
        private readonly IConsumer<Ignore, string> _consumer;

        /// <summary>
        /// Constructor
        /// </summary>
        public Worker()
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = Constantes.KafkaBootstrapServers,
                GroupId = "saldo-worker-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        }

        /// <summary>
        /// Ejecutar el servicio
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                _consumer.Subscribe(Constantes.KafkaTopic);

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

                                Console.WriteLine($"Deposito: {deposito.Propietario} {deposito.Monto}");

                                break;

                            case "DineroRetiradoEvento":
                                var retiro = JsonSerializer.Deserialize<DineroRetiradoEvento>(eventoJson);

                                Console.WriteLine($"Retiro: {retiro.Propietario} {retiro.Monto}");
                                break;

                            default:
                                break;
                        }
                    }
                    catch (OperationCanceledException) { break; }
                    catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
                }

            }, stoppingToken);
        }
    }
}
