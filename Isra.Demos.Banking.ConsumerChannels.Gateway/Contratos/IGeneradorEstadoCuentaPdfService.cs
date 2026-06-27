namespace Isra.Demos.Banking.ConsumerChannels.Gateway.Contratos
{
    /// <summary>
    /// Generador del estado de cuenta pdf
    /// </summary>
    public interface IGeneradorEstadoCuentaPdfService
    {
        /// <summary>
        /// Generar el pdf con el estado de cuenta
        /// </summary>
        /// <param name="idCuenta"></param>
        /// <returns></returns>
        Task<byte[]> GenerarEstadoCuentaPdf(Guid idCuenta);
    }
}
