using System.Diagnostics;

namespace Isra.Demos.Microservicios.CuentaMovimientos.Monitoreo
{
    /// <summary>
    /// Configuracion de open telemetry
    /// </summary>
    public static class MicroservicioTelemetry
    {
        /// <summary>
        /// Nombre del servicio
        /// </summary>
        public const string ServiceName = "Banco-Movimientos";

        /// <summary>
        /// Declaramos el ActivitySource como propiedad de solo lectura
        /// </summary>
        public static ActivitySource Source { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        static MicroservicioTelemetry()
        {
            Source = new ActivitySource("Isra.Demos.Microservicios.CuentaMovimientos");
        }
    }
}
