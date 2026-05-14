
using Isra.Demos.Microservicios.CuentaMovimientos.Modelo;
using Isra.Demos.Microservicios.CuentaMovimientos.Repositorio;
using Isra.Demos.Microservicios.Modelo;

namespace Isra.Demos.Microservicios.Servicios
{
    /// <summary>
    /// Servicio de operaciones sobre la cuenta bancaria. Este servicio es responsable de manejar las operaciones relacionadas con las cuentas bancarias, como la creación de cuentas, depósitos, retiros y la obtención de información de la cuenta. Utiliza un repositorio de eventos para almacenar y recuperar los eventos asociados a cada cuenta bancaria, lo que permite reconstruir el estado actual de la cuenta a partir de su historial de eventos. Este enfoque basado en eventos facilita la gestión de las operaciones en la cuenta bancaria y proporciona una forma eficiente de mantener un registro detallado de todas las transacciones realizadas en cada cuenta.
    /// </summary>
    public class CuentaBancariaService : ICuentaBancariaService
    {
        private readonly IRepositorioEventos _repositorioEventos;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eventRepository"></param>
        public CuentaBancariaService(
            IRepositorioEventos eventRepository)
        {
            _repositorioEventos = eventRepository;
        }

        /// <summary>
        /// Depositar un dinero
        /// </summary>
        /// <param name="cuentaId"></param>
        /// <param name="monto"></param>
        /// <param name="propietario"></param>
        /// <returns></returns>
        public async Task DepositarAsync(Guid cuentaId, decimal monto, string propietario)
        {
            var cuenta = await ObtenerCuentaAsync(cuentaId);
            cuenta.Depositar(monto, propietario);

            // Guardar los eventos generados
            foreach (var evento in cuenta.ObtenerEventos())
            {
                // se almacena el evento en el repositorio de eventos
                await _repositorioEventos.GuardarEventoAsync(evento);
            }

            cuenta.LimpiarEventos();
        }

        /// <summary>
        /// Retirar un dinero
        /// </summary>
        /// <param name="cuentaId"></param>
        /// <param name="monto"></param>
        /// <param name="propietario"></param>
        /// <returns></returns>
        public async Task RetirarAsync(Guid cuentaId, decimal monto, string propietario)
        {
            var cuenta = await ObtenerCuentaAsync(cuentaId);
            cuenta.Retirar(monto, propietario);

            // Guardar los eventos generados
            foreach (var evento in cuenta.ObtenerEventos())
            {
                await _repositorioEventos.GuardarEventoAsync(evento);
            }

            cuenta.LimpiarEventos();
        }

        /// <summary>
        /// Obtiene la info de la cuenta
        /// </summary>
        /// <param name="cuentaId"></param>
        /// <returns></returns>
        public async Task<CuentaBancaria> ObtenerCuentaAsync(Guid cuentaId)
        {
            var cuenta = new CuentaBancaria(cuentaId);
            var eventos = await _repositorioEventos.ObtenerEventosPorAgregadoAsync(cuentaId);

            if (eventos.Any())
            {
                cuenta.ReconstructirDesdeEventos(eventos);
            }

            return cuenta;
        }
    }
}
