using Dapper;
using Isra.Demos.Microservicios.Modelo;
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
        private readonly ISaldoRepositorio _saldoRepositorio;

        /// <summary>
        /// Constructor del repositorio de estado de cuenta, se encarga de inicializar la conexión a la base de datos utilizando la cadena de conexión definida en las constantes.
        /// </summary>
        public EstadoCuentaRepositorio(ISaldoRepositorio saldoRepositorio)
        {
            _connecionString = Constantes.SQLServerConnectionString;
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

            CuentaDto cuenta = new CuentaDto
            {
                AggregateId = aggregateId,
                Propietario = string.Empty,
                Saldo = 0
            };

            // obtiene los movimientos de la cuenta
            var movimientos =
                await connection.QueryAsync<CuentaMovimientoDto>(
                    "SELECT TipoMovimiento, Monto, FechaEvento, Propietario FROM MovimientosCuenta WHERE AggregateId = @AggregateId order by FechaEvento desc",
                    new { AggregateId = aggregateId });

            if (movimientos.Any())
            {
                // cargar el saldo
                cuenta.Saldo = await _saldoRepositorio.GetSaldoActualAsync(aggregateId);

                // propietario
                cuenta.Propietario = movimientos.First().Propietario;

                // movimientos
                cuenta.Movimientos = movimientos.ToArray();
            }

            return cuenta;
        }
    }
}
