using Dapper;
using Isra.Demos.Microservicios.WebApi.Contratos;
using Isra.Demos.Microservicios.WebApi.Modelo;
using Microsoft.Data.SqlClient;

namespace Isra.Demos.Microservicios.WebApi.Repositorio
{
    /// <summary>
    /// Repositorio de estado de cuenta, se encarga de obtener el estado de cuenta para un aggregateId específico. El estado de cuenta incluye el saldo actual y los movimientos asociados al aggregateId.
    /// </summary>
    public class EstadoCuentaRepositorio : IEstadoCuentaRepositorio
    {
        private readonly string _connecionString;
        private readonly string _cnnTblUsuarios;
        private readonly ISaldoRepositorio _saldoRepositorio;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor del repositorio de estado de cuenta, se encarga de inicializar la conexión a la base de datos utilizando la cadena de conexión definida en las constantes.
        /// </summary>
        public EstadoCuentaRepositorio(ISaldoRepositorio saldoRepositorio, IConfiguration configuration)
        {
            _configuration = configuration;
            _connecionString = _configuration.GetValue<string>("ConnectionStrings:SQLServerEstadoCuentaConnectionString");
            _cnnTblUsuarios = _configuration.GetValue<string>("ConnectionStrings:SqlServerBancoCuentasConnectionString");
            _saldoRepositorio = saldoRepositorio;
        }

        /// <summary>
        /// Obtiene el estado de cuenta para un aggregateId específico. El estado de cuenta incluye el saldo actual y los movimientos asociados al aggregateId.
        /// </summary>
        /// <param name="aggregateId"></param>
        /// <returns></returns>
        public async Task<CuentaDto> ObtenerEstadoCuentaAsync(Guid aggregateId)
        {
            using var connection = new SqlConnection(_connecionString);

            await connection.OpenAsync();

            // obtener los datos de la tabla TblCuentasUsuario
            var cuentaUsuario = await ObtenerCuentaAsync(aggregateId);

            // obtiene los movimientos de la cuenta
            var movimientos =
                await connection.QueryAsync<CuentaMovimientoDto>(
                    "SELECT TipoMovimiento, Monto, FechaEvento FROM MovimientosCuenta WHERE AggregateId = @AggregateId order by FechaEvento desc",
                    new { AggregateId = aggregateId });

            if (movimientos.Any())
            {
                // cargar el saldo
                cuentaUsuario.Saldo = await _saldoRepositorio.GetSaldoActualAsync(aggregateId);

                // movimientos
                cuentaUsuario.Movimientos = movimientos.ToArray();
            }

            return cuentaUsuario;
        }

        /// <summary>
        /// Obtiene los datos de la cuenta
        /// </summary>
        /// <param name="aggregateId"></param>
        /// <returns></returns>
        private async Task<CuentaDto> ObtenerCuentaAsync(Guid aggregateId)
        {
            using var connection = new SqlConnection(_cnnTblUsuarios);

            await connection.OpenAsync();

            // obtener los datos de la tabla TblCuentasUsuario
            var cuentaUsuario = await connection.QueryFirstOrDefaultAsync<CuentaDto>(
                "SELECT IdCuenta AggregateId, Propietario FROM TblCuentasUsuario WHERE IdCuenta = @AggregateId",
                new { AggregateId = aggregateId });

            return cuentaUsuario;
        }
    }
}
