using Dapper;
using Isra.Demos.Microservicios.WebApi.Contratos;
using Npgsql;

namespace Isra.Demos.Microservicios.WebApi.Repositorio
{
    /// <summary>
    /// Repositorio que implementa la interfaz ISaldoRepositorio para acceder a la base de datos PostgreSQL y obtener el saldo actualizado de una cuenta después de una transacción. Utiliza Dapper para ejecutar consultas SQL de manera eficiente.
    /// </summary>
    public class SaldoRepositorio : ISaldoRepositorio
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor que inicializa la cadena de conexión a la base de datos PostgreSQL utilizando una constante definida en el proyecto.
        /// </summary>
        public SaldoRepositorio(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetValue<string>("ConnectionStrings:PostgresSaldoConnection");
        }

        /// <summary>
        /// Obtiene el saldo de la cuenta actualizada después de una transacción, utilizando el ID de la cuenta.
        /// </summary>
        /// <param name="cuentaId"></param>
        /// <returns></returns>
        public async Task<decimal> GetSaldoActualAsync(Guid cuentaId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                return await connection.QuerySingleAsync<decimal>(
                    "SELECT saldo FROM cuentas.saldos_cuenta WHERE id = @cuentaId",
                    new { cuentaId });
            }
        }
    }
}
