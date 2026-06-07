using Dapper;
using Isra.Demos.Microservicios.CuentaMovimientos.Modelo;
using Microsoft.Data.SqlClient;
using Polly;

namespace Isra.Demos.Microservicios.CuentaMovimientos.Consultas
{
    /// <summary>
    /// Consulta para obtener la información de una cuenta por su ID desde la base de datos SQL Server.
    /// </summary>
    public class ObtenerCuentaPorIdConsulta
    {
        private readonly string _connectionString;
        private readonly ResiliencePipeline _resiliencePipeline;

        /// <summary>
        /// Constructor que recibe la configuración para obtener la cadena de conexión a la base de datos SQL Server.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="resiliencePipeline"></param>
        public ObtenerCuentaPorIdConsulta(
            IConfiguration configuration,
            ResiliencePipeline resiliencePipeline)
        {
            _connectionString = configuration.GetConnectionString("SqlServerBancoCuentasConnectionString");
            _resiliencePipeline = resiliencePipeline;
        }

        /// <summary>
        /// Ejecuta la consulta para obtener la información de una cuenta por su ID.
        /// </summary>
        /// <param name="idCuenta"></param>
        /// <returns></returns>
        public async Task<CuentaBancaria> EjecutarAsync(Guid idCuenta)
        {
            return await _resiliencePipeline.ExecuteAsync(async token =>
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var query = "SELECT IdCuenta Id FROM TblCuentasUsuario WHERE IdCuenta = @Id and Estatus = 1";
                    return await connection.QueryFirstOrDefaultAsync<CuentaBancaria>(
                        query,
                         new { Id = idCuenta }
                    );
                }
            });
        }
    }
}
