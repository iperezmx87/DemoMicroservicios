namespace Isra.Demos.EventSource.Models
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
    }
}
