namespace Isra.Demos.Microservicios.Modelo
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
        /// Propietario de la cuenta
        /// </summary>
        public string Propietario { get; set; }

        /// <summary>
        /// Constructor para inicializar el evento con los datos necesarios
        /// </summary>
        /// <param name="id"></param>
        /// <param name="monto"></param>
        /// <param name="version"></param>
        /// <param name="propietario"></param>
        public DineroDepositadoEvento(Guid id, decimal monto, int version, string propietario)
        {
            AggregateId = id;
            Monto = monto;
            Version = version;
            Propietario = propietario;
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
        /// Propietario de la cuenta
        /// </summary>
        public string Propietario { get; set; }

        /// <summary>
        /// Constructor para inicializar el evento con los datos necesarios
        /// </summary>
        /// <param name="id"></param>
        /// <param name="monto"></param>
        /// <param name="version"></param>
        /// <param name="propietario"></param>
        public DineroRetiradoEvento(Guid id, decimal monto, int version, string propietario)
        {
            AggregateId = id;
            Monto = monto;
            Version = version;
            Propietario = propietario;
        }

        /// <summary>
        /// constructor sin parámetros para permitir la deserialización del evento desde JSON, ya que algunas bibliotecas de serialización requieren un constructor sin parámetros para crear una instancia del objeto antes de asignar las propiedades. Este constructor es esencial para garantizar que el proceso de deserialización funcione correctamente, permitiendo que el evento se reconstruya a partir de su representación JSON sin problemas.
        /// </summary>
        public DineroRetiradoEvento()
        {
            
        }
    }
}
