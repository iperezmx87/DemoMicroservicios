
using Isra.Demos.Microservicios.CuentaMovimientos.Consultas;
using Isra.Demos.Microservicios.CuentaMovimientos.Modelo;
using Isra.Demos.Microservicios.CuentaMovimientos.Repositorio;

namespace Isra.Demos.Microservicios.CuentaMovimientos.Servicios
{
    /// <summary>
    /// Servicio de operaciones sobre la cuenta bancaria. Este servicio es responsable de manejar las operaciones relacionadas con las cuentas bancarias, como la creación de cuentas, depósitos, retiros y la obtención de información de la cuenta. Utiliza un repositorio de eventos para almacenar y recuperar los eventos asociados a cada cuenta bancaria, lo que permite reconstruir el estado actual de la cuenta a partir de su historial de eventos. Este enfoque basado en eventos facilita la gestión de las operaciones en la cuenta bancaria y proporciona una forma eficiente de mantener un registro detallado de todas las transacciones realizadas en cada cuenta.
    /// </summary>
    public class CuentaBancariaService : ICuentaBancariaService
    {
        private readonly IRepositorioEventos _repositorioEventos;
        private readonly ObtenerCuentaPorIdConsulta _obtenerCuentaPorIdConsulta;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eventRepository"></param>
        /// <param name="obtenerCuentaPorIdConsulta"></param>
        public CuentaBancariaService(
            IRepositorioEventos eventRepository,
            ObtenerCuentaPorIdConsulta obtenerCuentaPorIdConsulta)
        {
            _repositorioEventos = eventRepository;
            _obtenerCuentaPorIdConsulta = obtenerCuentaPorIdConsulta;
        }

        /// <summary>
        /// Depositar un dinero
        /// </summary>
        /// <param name="cuentaId"></param>
        /// <param name="monto"></param>
        /// <returns></returns>
        public async Task DepositarAsync(Guid cuentaId, decimal monto)
        {
            var cuentaConsulta = await _obtenerCuentaPorIdConsulta.EjecutarAsync(cuentaId);

            if (cuentaConsulta == null)
                throw new InvalidDataException("El número de cuenta no existe o no está activa.");

            var cuenta = await ObtenerCuentaAsync(cuentaId);

            cuenta.Depositar(monto);

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
        /// <returns></returns>
        public async Task RetirarAsync(Guid cuentaId, decimal monto)
        {
            var cuentaConsulta = await _obtenerCuentaPorIdConsulta.EjecutarAsync(cuentaId);

            if (cuentaConsulta == null)
                throw new InvalidDataException("El número de cuenta no existe o no está activa.");

            var cuenta = await ObtenerCuentaAsync(cuentaId);

            if (cuenta.Version == 0)
                throw new InvalidDataException("La cuenta no ha sido inicializada.");

            cuenta.Retirar(monto);

            // Guardar los eventos generados
            foreach (var evento in cuenta.ObtenerEventos())
            {
                await _repositorioEventos.GuardarEventoAsync(evento);
            }

            cuenta.LimpiarEventos();
        }

        /// <summary>
        /// Transfiere dinero de una cuenta a otra
        /// Se efectúa solamente el evento del dinero enviado, es otro servicio quien recibe el evento y procesa el deposito
        /// </summary>
        /// <param name="cuentaOrigenId"></param>
        /// <param name="cuentaDestinoId"></param>
        /// <param name="monto"></param>
        /// <returns></returns>
        public async Task TransferirAsync(Guid cuentaOrigenId, Guid cuentaDestinoId, decimal monto)
        {
            var cuentaConsulta = await _obtenerCuentaPorIdConsulta.EjecutarAsync(cuentaOrigenId);

            if (cuentaConsulta == null)
                throw new InvalidDataException("El número de cuenta origen no existe o no está activa.");

            var cuentaOrigen = await ObtenerCuentaAsync(cuentaOrigenId);

            if (cuentaOrigen.Version == 0)
                throw new InvalidDataException("La cuenta de origen no ha sido inicializada.");

            await cuentaOrigen.TransferirDineroACuentaAsync(cuentaDestinoId, monto);

            foreach (var evento in cuentaOrigen.ObtenerEventos())
            {
                await _repositorioEventos.GuardarEventoAsync(evento);
            }
            cuentaOrigen.LimpiarEventos();
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
