using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Isra.Demos.Microservicios.CuentaMovimientos.Modelo
{
    /// <summary>
    /// Evento de dinero depositado
    /// </summary>
    public class DineroDepositadoEvento : EventoBase
    {
        /// <summary>
        /// Monto a depositar
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Constructor para inicializar el evento con los datos necesarios
        /// </summary>
        /// <param name="id"></param>
        /// <param name="monto"></param>
        /// <param name="version"></param>
        public DineroDepositadoEvento(Guid id, decimal monto, int version)
        {
            AggregateId = id;
            Monto = monto;
            Version = version;
        }

        /// <summary>
        /// constructor sin parámetros para permitir la deserialización del evento desde JSON, ya que algunas bibliotecas de serialización requieren un constructor sin parámetros para crear una instancia del objeto antes de asignar las propiedades. Este constructor es esencial para garantizar que el proceso de deserialización funcione correctamente, permitiendo que el evento se reconstruya a partir de su representación JSON sin problemas.
        /// </summary>
        public DineroDepositadoEvento()
        {
        }
    }

    /// <summary>
    /// Evento de dinero retirado
    /// </summary>
    public class DineroRetiradoEvento : EventoBase
    {
        /// <summary>
        /// Monto a retirar
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Constructor para inicializar el evento con los datos necesarios
        /// </summary>
        /// <param name="id"></param>
        /// <param name="monto"></param>
        /// <param name="version"></param>
        public DineroRetiradoEvento(Guid id, decimal monto, int version)
        {
            AggregateId = id;
            Monto = monto;
            Version = version;
        }

        /// <summary>
        /// constructor sin parámetros para permitir la deserialización del evento desde JSON, ya que algunas bibliotecas de serialización requieren un constructor sin parámetros para crear una instancia del objeto antes de asignar las propiedades. Este constructor es esencial para garantizar que el proceso de deserialización funcione correctamente, permitiendo que el evento se reconstruya a partir de su representación JSON sin problemas.
        /// </summary>
        public DineroRetiradoEvento()
        {
        }
    }

    /// <summary>
    /// Evento que se dispara cuando se ha lanzado una orden de transferencia, este evento es importante para el proceso de transferencia de fondos entre cuentas, ya que permite registrar la acción de iniciar una transferencia y proporciona información relevante como el monto a transferir y la cuenta destino. Al emitir este evento, se puede desencadenar una serie de acciones en otros servicios o componentes del sistema, como la validación de fondos, la actualización del estado de la cuenta origen y destino, y la notificación a los usuarios involucrados en la transferencia. Además, este evento contribuye a mantener un registro histórico de las transferencias realizadas, lo que es fundamental para la auditoría y el análisis de transacciones en el sistema.
    /// </summary>
    public class TransferenciaRealizadaEvento : EventoBase
    {
        /// <summary>
        /// Monto a transferir
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Cuenta destino a la que se realizará la transferencia. Esta propiedad es crucial para identificar la cuenta receptora de los fondos transferidos, lo que permite que el sistema pueda actualizar correctamente el saldo de la cuenta destino y garantizar que la transferencia se procese de manera adecuada. Al incluir esta información en el evento, se facilita la trazabilidad de las transacciones y se asegura que todas las partes involucradas en la transferencia tengan acceso a los detalles necesarios para su correcta ejecución y registro.
        /// </summary>
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid CuentaDestinoId { get; set; }

        /// <summary>
        /// Constructor para inicializar el evento con los datos necesarios. Este constructor es esencial para garantizar que el evento se cree con toda la información relevante desde el momento de su instanciación, lo que facilita su uso en el proceso de transferencia de fondos. Al proporcionar un constructor que acepta los parámetros necesarios, se asegura que el evento se construya de manera consistente y que todos los datos importantes estén disponibles para su procesamiento posterior en el sistema.
        /// </summary>
        /// <param name="id">Id de cuenta origen</param>
        /// <param name="monto">Monto a transferir</param>
        /// <param name="cuentaDestinoId">Id de cuenta destino</param>
        /// <param name="version">Versión del evento</param>
        public TransferenciaRealizadaEvento(Guid id, decimal monto, Guid cuentaDestinoId, int version)
        {
            AggregateId = id;
            Monto = monto;
            CuentaDestinoId = cuentaDestinoId;
            Version = version;
        }

        /// <summary>
        /// Constructor vacio para json
        /// </summary>
        public TransferenciaRealizadaEvento()
        {
        }
    }

    /// <summary>
    /// Evento de transferencia recibida, este evento se dispara cuando una transferencia ha sido recibida en la cuenta destino, lo que indica que los fondos han sido acreditados correctamente en la cuenta receptora. Este evento es fundamental para el proceso de transferencia de fondos, ya que permite registrar la acción de recibir una transferencia y proporciona información relevante como el monto recibido. Al emitir este evento, se puede desencadenar una serie de acciones en otros servicios o componentes del sistema, como la actualización del estado de la cuenta destino, la notificación a los usuarios involucrados en la transferencia y la generación de registros
    /// </summary>
    public class TransferenciaRecibidaEvento : EventoBase
    {
        /// <summary>
        /// Monto recibido
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Cuenta destino a la que se realizará la transferencia. Esta propiedad es crucial para identificar la cuenta receptora de los fondos transferidos, lo que permite que el sistema pueda actualizar correctamente el saldo de la cuenta destino y garantizar que la transferencia se procese de manera adecuada. Al incluir esta información en el evento, se facilita la trazabilidad de las transacciones y se asegura que todas las partes involucradas en la transferencia tengan acceso a los detalles necesarios para su correcta ejecución y registro.
        /// </summary>
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid CuentaDestinoId { get; set; }

        /// <summary>
        /// Constructor para inicializar el evento con los datos necesarios. Este constructor es esencial para garantizar que el evento se cree con toda la información relevante desde el momento de su instanciación, lo que facilita su uso en el proceso de recepción de transferencias. Al proporcionar un constructor que acepta los parámetros necesarios, se asegura que el evento se construya de manera consistente y que todos los datos importantes estén disponibles para su procesamiento posterior en el sistema.
        /// </summary>
        /// <param name="id">Id de cuenta destino</param>
        /// <param name="monto">Monto recibido</param>
        /// <param name="version">Versión del evento</param>
        public TransferenciaRecibidaEvento(Guid id, decimal monto, int version)
        {
            AggregateId = id;
            Monto = monto;
            Version = version;
        }

        /// <summary>
        /// Constructor vacio para json
        /// </summary>
        public TransferenciaRecibidaEvento()
        {
        }
    }

    /// <summary>
    /// Cuando una trasnferencia en la cuenta destino falle, se hará un rollback de la transferencia, lo que implica que se revertirá cualquier cambio realizado en la cuenta origen y se notificará a los usuarios involucrados sobre el fallo de la transferencia. Este evento es crucial para mantener la integridad de las transacciones y garantizar que el sistema pueda manejar adecuadamente los errores que puedan surgir durante el proceso de transferencia. Al emitir este evento, se puede desencadenar una serie de acciones correctivas, como la restauración del saldo original en la cuenta origen y la generación de alertas para los usuarios afectados, lo que contribuye a mejorar la experiencia del usuario y a mantener la confianza en el sistema.
    /// </summary>
    public class TransferenciaDevueltaEvento : EventoBase
    {
        /// <summary>
        /// Monto a devolver
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// El id de la transferencia original que se está devolviendo, esta propiedad es fundamental para identificar la transferencia específica que ha fallado y que se está revirtiendo. Al incluir el ID de la transferencia original en el evento, se facilita la traz
        /// </summary>
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid IdTransferenciaOrigen { get; set; }

        /// <summary>
        /// cuenta de donde salio la transferencia original
        /// </summary>
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid CuentaOrigenId { get; set; }

        /// <summary>
        /// constructor para inicializar el evento con los datos necesarios. Este constructor es esencial para garantizar que el evento se cree con toda la información relevante desde el momento de su instanciación, lo que facilita su uso en el proceso de manejo de errores durante las transferencias. Al proporcionar un constructor que acepta los parámetros necesarios, se asegura que el evento se construya de manera consistente y que todos los datos importantes estén disponibles para su procesamiento posterior en el sistema, permitiendo así una gestión efectiva de las transferencias fallidas y la restauración del estado original de las cuentas involucradas.
        /// </summary>
        /// <param name="id">Id cuenta receptora de la transferencia</param>
        /// <param name="idTransferenciaOrigen">Id de la transferencia original</param>
        /// <param name="monto">Monto a devolver</param>
        /// <param name="cuentaOrigenId">Id de la cuenta de origen</param>
        /// <param name="version">Versión del evento</param>
        public TransferenciaDevueltaEvento(Guid id, Guid idTransferenciaOrigen, decimal monto, Guid cuentaOrigenId, int version)
        {
            AggregateId = id;
            IdTransferenciaOrigen = idTransferenciaOrigen;
            Monto = monto;
            CuentaOrigenId = cuentaOrigenId;
            Version = version;
        }

        /// <summary>
        /// constructor vacio para json, este constructor es necesario para permitir la deserialización del evento desde JSON, ya que algunas bibliotecas de serialización requieren un constructor sin parámetros para crear una instancia del objeto antes de asignar las propiedades. Este constructor es esencial para garantizar que el proceso de deserialización funcione correctamente, permitiendo que el evento se reconstruya a partir de su representación JSON sin problemas.
        /// </summary>
        public TransferenciaDevueltaEvento()
        {
        }
    }
}