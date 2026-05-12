using Confluent.Kafka;
using Isra.Demos.Microservicios.Modelo;
using Microsoft.Extensions.Primitives;
using System.Text.Json;

namespace Isra.Demos.Microservicios.Servicios
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
        public async Task<Tuple<string, string>> PublicarDineroDepositadoEventoAsync(
            DineroDepositadoEvento evento)
        {
            var mensaje = JsonSerializer.Serialize(evento);

            return await PublicarEventoAsync(evento.EventId.ToString(), mensaje);
        }

        /// <summary>
        /// Publica el mensaje en la cola
        /// </summary>
        /// <param name="evento"></param>
        /// <returns></returns>
        public async Task<Tuple<string, string>> PublicarDineroRetiradoEventoAsync(
            DineroRetiradoEvento evento)
        {
            var mensaje = JsonSerializer.Serialize(evento);

            return await PublicarEventoAsync(evento.EventId.ToString(), mensaje);
        }

        private async Task<Tuple<string, string>> PublicarEventoAsync(string llave, string mensaje)
        {
            var resultado =
                 await _producer.ProduceAsync(Constantes.KafkaTopic, new Message<string, string>
                 {
                     Key = llave,
                     Value = mensaje
                 });

            return Tuple.Create(resultado.Topic, Enum.GetName(resultado.Status));
        }
    }
}
