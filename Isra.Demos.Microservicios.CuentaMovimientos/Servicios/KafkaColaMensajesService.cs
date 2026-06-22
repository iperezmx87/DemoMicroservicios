using Confluent.Kafka;
using Isra.Demos.Banking.CurrentAccount.Modelo;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Isra.Demos.Banking.CurrentAccount.Servicios
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

        /// <summary>
        /// Publica el mensaje de devolución de transferencia en la cola
        /// </summary>
        /// <param name="evento"></param>
        /// <returns></returns>
        public async Task<Tuple<string, string>> PublicarTransferenciaRealizadaEventoAsync(
            TransferenciaRealizadaEvento evento)
        {
            var mensaje = JsonSerializer.Serialize(evento);

            return await PublicarEventoAsync(evento.EventId.ToString(), mensaje);
        }

        private async Task<Tuple<string, string>> PublicarEventoAsync(string llave, string mensaje)
        {
            // 4. Creamos el objeto Message e inicializamos su colección de Headers
            var kafkaMessage = new Message<string, string>
            {
                Key = llave,
                Value = mensaje,
                Headers = new Headers()
            };

            // 5. Si hay una actividad (Span) activa en este hilo (la que abrimos en el Outbox BackgroundService),
            // inyectamos su TraceId de forma síncrona en los headers binarios de Kafka usando el estándar W3C.
            if (Activity.Current != null)
            {
                var propagationContext = new PropagationContext(Activity.Current.Context, Baggage.Current);

                Propagators.DefaultTextMapPropagator.Inject(propagationContext, kafkaMessage.Headers, (headers, key, value) =>
                {
                    // Kafka maneja los valores de los headers estrictamente como arreglos de bytes (byte[])
                    headers.Add(key, Encoding.UTF8.GetBytes(value));
                });
            }

            // 6. Publicamos el mensaje enriquecido al tópico correspondiente
            var resultado = await _producer.ProduceAsync(
                 _configuration.GetValue<string>("Kafka:Topic"),
                 kafkaMessage);

            return Tuple.Create(resultado.Topic, Enum.GetName(resultado.Status));
        }
    }
}
