using Dapper;
using Isra.Demos.Banking.ConsumerChannels.Gateway.Contratos;
using Npgsql;
using Polly;

namespace Isra.Demos.Banking.ConsumerChannels.Gateway.Repositorio
{
    /// <summary>
    /// Repositorio que implementa la interfaz ISaldoRepositorio para acceder a la base de datos PostgreSQL y obtener el saldo actualizado de una cuenta después de una transacción. Utiliza Dapper para ejecutar consultas SQL de manera eficiente.
    /// </summary>
    public class SaldoRepositorio : ISaldoRepositorio
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        private readonly ResiliencePipeline _resiliencePipeline;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="resiliencePipeline"></param>
        public SaldoRepositorio(IConfiguration configuration,
            ResiliencePipeline resiliencePipeline)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetValue<string>("ConnectionStrings:PostgresSaldoConnection");
            _resiliencePipeline = resiliencePipeline;
        }

        /// <summary>
        /// Obtiene el saldo de la cuenta actualizada después de una transacción, utilizando el ID de la cuenta.
        /// </summary>
        /// <param name="cuentaId"></param>
        /// <returns></returns>
        public async Task<decimal> GetSaldoActualAsync(Guid cuentaId)
        {
            return await _resiliencePipeline.ExecuteAsync(async token =>
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    return await connection.QuerySingleAsync<decimal>(
                        "SELECT saldo FROM cuentas.saldos_cuenta WHERE id = @cuentaId",
                        new { cuentaId });
                }
            });
        }
    }
}
