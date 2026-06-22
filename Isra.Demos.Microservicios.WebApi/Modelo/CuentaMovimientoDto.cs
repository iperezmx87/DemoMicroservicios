namespace Isra.Demos.Banking.ConsumerChannels.Gateway.Modelo
{
    /// <summary>
    /// Dto de la cuenta bancaria, se utiliza para mostrar la información de la cuenta bancaria en el estado de cuenta
    /// </summary>
    public class CuentaDto
    {
        /// <summary>
        /// Id de la cuenta
        /// </summary>
        public Guid AggregateId { get; set; }

        /// <summary>
        /// Dueño de la cuenta
        /// </summary>
        public string Propietario { get; set; }

        /// <summary>
        /// Saldo actual
        /// </summary>
        public decimal Saldo { get; set; }

        /// <summary>
        /// Movimientos de la cuenta
        /// </summary>
        public CuentaMovimientoDto[] Movimientos { get; set; }
    }

    /// <summary>
    /// Movimiento de la cuenta, retiro o deposito, se utiliza para mostrar la información de los movimientos de la cuenta bancaria en el estado de cuenta
    /// </summary>
    public class CuentaMovimientoDto
    {
        /// <summary>
        /// Monto del movimiento
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Tipo del movimiento, retiro o deposito
        /// </summary>
        public string TipoMovimiento { get; set; }

        /// <summary>
        /// Se coloca el motivo de la devolución
        /// </summary>
        public string MotivoDevolucion { get; set; }

        /// <summary>
        /// Se coloca el propietario de la transferencia destino
        /// </summary>
        public string PropietarioTransferenciaDestino { get; set; }

        /// <summary>
        /// Fecha del movimiento
        /// </summary>
        public DateTimeOffset FechaEvento { get; set; }
    }
}
