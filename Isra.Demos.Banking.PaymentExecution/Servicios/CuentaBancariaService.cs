using Isra.Demos.Banking.PaymentExecution.Consultas;
using Isra.Demos.Banking.PaymentExecution.Modelo;
using Isra.Demos.Banking.PaymentExecution.Repositorio;

namespace Isra.Demos.Banking.PaymentExecution.Servicios
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
        /// Procesar la transferencia recibida, aplicar reglas de negocio
        /// </summary>
        /// <param name="cuentaDestinoId"></param>
        /// <param name="monto"></param>
        /// <returns></returns>
        public async Task RecibirTransferenciaAsync(Guid cuentaDestinoId, decimal monto)
        {
            var cuentaConsulta = await _obtenerCuentaPorIdConsulta.EjecutarAsync(cuentaDestinoId);

            if (cuentaConsulta == null)
                throw new InvalidDataException("El número de cuenta no existe o no está activa.");

            var cuentaDestino = await ObtenerCuentaAsync(cuentaDestinoId);

            if (cuentaDestino.Version == 0)
                throw new InvalidDataException("La cuenta no ha sido inicializada.");

            await cuentaDestino.RecibirTransferenciaAsync(cuentaDestinoId, monto);

            foreach (var evento in cuentaDestino.ObtenerEventos())
            {
                await _repositorioEventos.GuardarEventoAsync(evento);
            }

            cuentaDestino.LimpiarEventos();
        }

        /// <summary>
        /// Efectúa la devolución de una transferencia, aplicando las reglas de negocio correspondientes, como verificar que la transferencia original exista, que el motivo de devolución sea válido y que el monto a devolver no exceda el monto original de la transferencia. Este método también se encarga de generar los eventos necesarios para reflejar la devolución en el sistema y garantizar que el estado de la cuenta bancaria se actualice correctamente.
        /// </summary>
        /// <param name="idTransferenciaOrigen"></param>
        /// <param name="cuentaOrigenId"></param>
        /// <param name="motivoDevolucion"></param>
        /// <param name="monto"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public async Task DevolverTransferenciaAsync(
            Guid idTransferenciaOrigen,
            Guid cuentaOrigenId,
            string motivoDevolucion,
            decimal monto)
        {
            var cuentaConsulta = await _obtenerCuentaPorIdConsulta.EjecutarAsync(cuentaOrigenId);

            if (cuentaConsulta == null)
                throw new InvalidDataException("El número de cuenta no existe o no está activa.");

            var cuentaOrigen = await ObtenerCuentaAsync(cuentaOrigenId);

            if (cuentaOrigen.Version == 0)
                throw new InvalidDataException("La cuenta no ha sido inicializada.");

            await cuentaOrigen.DevolverDineroTransferenciaAsync(
                idTransferenciaOrigen, cuentaOrigenId, motivoDevolucion, monto);

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
