namespace Isra.Demos.Microservicios.WebApi.Contratos
{
    /// <summary>
    /// Interface de repositorio para consultar el saldo actual de una cuenta. Permite abstraer la lógica de acceso a datos y facilitar el mantenimiento del código.
    /// Conecta a Postgres
    /// </summary>
    public interface ISaldoRepositorio
    {
        /// <summary>
        /// Consulta rápida para el saldo actual
        /// </summary>
        /// <param name="cuentaId"></param>
        /// <returns></returns>
        Task<decimal> GetSaldoActualAsync(Guid cuentaId);
    }
}
