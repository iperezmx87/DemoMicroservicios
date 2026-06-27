namespace Isra.Demos.Banking.CurrentAccount.Modelo
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
    }
}
