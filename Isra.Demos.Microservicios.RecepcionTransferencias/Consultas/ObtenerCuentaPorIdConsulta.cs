using Microsoft.Data.SqlClient;
using Dapper;
using Isra.Demos.Microservicios.RecepcionTransferencias.Modelo;

namespace Isra.Demos.Microservicios.RecepcionTransferencias.Consultas
{
    /// <summary>
    /// Consulta para obtener la información de una cuenta por su ID desde la base de datos SQL Server.
    /// </summary>
    public class ObtenerCuentaPorIdConsulta
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor que recibe la configuración para obtener la cadena de conexión a la base de datos SQL Server.
        /// </summary>
        /// <param name="configuration"></param>
        public ObtenerCuentaPorIdConsulta(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SqlServerBancoCuentasConnectionString");
        }

        /// <summary>
        /// Ejecuta la consulta para obtener la información de una cuenta por su ID.
        /// </summary>
        /// <param name="idCuenta"></param>
        /// <returns></returns>
        public async Task<CuentaBancaria> EjecutarAsync(Guid idCuenta)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT IdCuenta Id FROM TblCuentasUsuario WHERE IdCuenta = @Id and Estatus = 1";
                return await connection.QueryFirstOrDefaultAsync<CuentaBancaria>(
                    query,
                     new { Id = idCuenta }
                );
            }
        }
    }
}
