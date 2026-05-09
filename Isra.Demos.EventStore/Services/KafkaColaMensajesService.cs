using Confluent.Kafka;
using System.Text.Json;

namespace Isra.Demos.EventStore.Services
{
    /// <summary>
    /// Implementación de la cola de mensajes
    /// </summary>
    public class KafkaColaMensajesService
        : IColaMensajesService
    {
        private readonly IProducer<string, string> _producer;

        /// <summary>
        /// Constructor que inicializa el productor de Kafka
        /// </summary>
        public KafkaColaMensajesService()
        {
            var config = new ProducerConfig
            {
                BootstrapServers = Constantes.KafkaBootstrapServers
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        /// <summary>
        /// Publica el mensaje en la cola
        /// </summary>
        /// <param name="evento"></param>
        /// <returns></returns>
        public async Task PublicarEventoAsync(EventoBase evento)
        {
            var mensaje = JsonSerializer.Serialize(evento);

            await _producer.ProduceAsync(Constantes.KafkaTopic, new Message<string, string>
            {
                Key = evento.EventId.ToString(),
                Value = mensaje
            });
        }
    }
}
