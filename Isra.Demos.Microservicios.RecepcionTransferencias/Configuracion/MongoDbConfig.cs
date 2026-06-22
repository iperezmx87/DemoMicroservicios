using Isra.Demos.Banking.PaymentExecution.Modelo;
using MongoDB.Bson.Serialization;

namespace Isra.Demos.Banking.PaymentExecution.Configuracion
{
    /// <summary>
    /// Clase de configuración para MongoDB, donde se registran los mapeos de las clases de eventos para que MongoDB pueda serializarlas y deserializarlas correctamente.
    /// </summary>
    public static class MongoDbConfig
    {
        /// <summary>
        /// Registrar mapeos de las clases de eventos para MongoDB. Esto es necesario para que MongoDB pueda serializar y deserializar correctamente las clases que heredan de EventoBase, como DineroDepositado y DineroRetirado.
        /// </summary>
        public static void RegistrarMapeos()
        {
            // Esto permite que MongoDB guarde y lea las clases que heredan de EventoBase
            BsonClassMap.RegisterClassMap<EventoBase>(cm =>
            {
                cm.AutoMap();
                cm.SetIsRootClass(true);
            });

            BsonClassMap.RegisterClassMap<DineroDepositadoEvento>();

            BsonClassMap.RegisterClassMap<DineroRetiradoEvento>();

            BsonClassMap.RegisterClassMap<TransferenciaRealizadaEvento>();

            BsonClassMap.RegisterClassMap<TransferenciaRecibidaEvento>();

            BsonClassMap.RegisterClassMap<TransferenciaDevueltaEvento>();
        }
    }
}
