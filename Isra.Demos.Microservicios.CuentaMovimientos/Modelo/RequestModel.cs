namespace Isra.Demos.Microservicios.CuentaMovimientos.Modelo
{
    /// <summary>
    /// Solicitud para crear una cuenta
    /// </summary>
    public class CrearCuentaRequest
    {
        /// <summary>
        /// Id de la cuenta
        /// </summary>
        public Guid CuentaId { get; set; }
    }

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
}
