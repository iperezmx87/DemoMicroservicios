namespace Isra.Demos.Microservicios.CuentaMovimientos.Modelo
{
    /// <summary>
    /// Solicitud para operaciones monetarias
    /// </summary>
    public class OperacionMonetariaRequest
    {
        /// <summary>
        /// Monto de la 
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Propietario de la cuenta
        /// </summary>
        public string Propietario { get; set; }
    }

    /// <summary>
    /// Solicitud para transferir dinero a otra cuenta
    /// </summary>
    public class TransferenciaRequest
    {
        /// <summary>
        /// Id de la cuenta que envía el dinero
        /// </summary>
        public Guid CuentaOrigenId { get; set; }

        /// <summary>
        /// Id de la cuenta a la que se le transferirá el dinero
        /// </summary>
        public Guid CuentaDestinoId { get; set; }

        /// <summary>
        /// Monto a transferir
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Propietario de la cuenta que envía el dinero
        /// </summary>
        public string PropietarioOrigen { get; set; }

        /// <summary>
        /// Propietario de la cuenta que recibe el dinero (opcional)
        /// </summary>
        public string PropietarioDestino { get; set; }
    }
}
