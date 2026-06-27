using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Isra.Demos.Banking.CurrentAccount.Modelo
{
    /// <summary>
    /// Mensaje de salida del patron Outbox, representa un evento que se va a publicar en Kafka.
    /// </summary>
    public class MensajeSalida
    {
        /// <summary>
        /// Id del evento
        /// </summary>
        [BsonId]
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Topico del evento, por ejemplo "movimiento.creado" o "movimiento.actualizado". Esto se puede usar para enrutar el mensaje en Kafka.
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// Mensaje a enviar a Kafka, se recomienda que sea un JSON con la información relevante del evento, por ejemplo:
        /// </summary>
        public string Payload { get; set; } // El evento serializado en JSON

        /// <summary>
        /// Fecha de ocurrencia del evento, se recomienda usar UTC para evitar problemas de zona horaria. Esto se puede usar para ordenar los eventos en Kafka.
        /// </summary>
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Bandera que indica si el mensaje ya fue procesado y enviado a Kafka. Esto es importante para evitar enviar el mismo evento varias veces en caso de fallos o reinicios del servicio.
        /// </summary>
        public bool Processed { get; set; } = false; // Para saber si ya se envió a Kafka

        /// <summary>
        /// Id del trace de open telemetry
        /// </summary>
        public string TraceId { get; set; }
    }
}
