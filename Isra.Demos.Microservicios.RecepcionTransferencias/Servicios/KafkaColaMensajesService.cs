using Confluent.Kafka;
using Isra.Demos.Microservicios.RecepcionTransferencias.Modelo;
using Isra.Demos.Microservicios.RecepcionTransferencias.Servicios;
using System.Text.Json;

namespace Isra.Demos.Microservicios.RecepcionTransferencias.Servicios
{
    /// <summary>
    /// Implementación de la cola de mensajes
    /// </summary>
    public class KafkaColaMensajesService
        : IColaMensajesService
    {
        private readonly IProducer<string, string> _producer;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor que inicializa el productor de Kafka
        /// </summary>
        public KafkaColaMensajesService(IConfiguration configuration)
        {
            _configuration = configuration;

            var config = new ProducerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"]
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        /// <summary>
        /// Publica el mensaje de envio de transferencia en la cola
        /// </summary>
        /// <param name="evento"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<Tuple<string, string>> PublicarTransferenciaDevueltaEventoAsync(
            TransferenciaDevueltaEvento evento)
        {
            var mensaje = JsonSerializer.Serialize(evento);

            return await PublicarEventoAsync(evento.EventId.ToString(), mensaje);
        }

        /// <summary>
        /// Publica el mensaje de devolución de transferencia en la cola
        /// </summary>
        /// <param name="evento"></param>
        /// <returns></returns>
        public async Task<Tuple<string, string>> PublicarTransferenciaRecibidaEventoAsync(
            TransferenciaRecibidaEvento evento)
        {
            var mensaje = JsonSerializer.Serialize(evento);

            return await PublicarEventoAsync(evento.EventId.ToString(), mensaje);
        }

        private async Task<Tuple<string, string>> PublicarEventoAsync(string llave, string mensaje)
        {
            var resultado =
                 await _producer.ProduceAsync(
                     _configuration.GetValue<string>("Kafka:Topic"), new Message<string, string>
                     {
                         Key = llave,
                         Value = mensaje
                     });

            return Tuple.Create(resultado.Topic, Enum.GetName(resultado.Status));
        }
    }
}
