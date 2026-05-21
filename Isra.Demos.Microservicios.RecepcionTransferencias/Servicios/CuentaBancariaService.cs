using Isra.Demos.Microservicios.RecepcionTransferencias.Modelo;
using Isra.Demos.Microservicios.RecepcionTransferencias.Repositorio;

namespace Isra.Demos.Microservicios.RecepcionTransferencias.Servicios
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
        /// Obtiene la info de la cuenta
        /// </summary>
        /// <param name="cuentaId"></param>
        /// <returns></returns>
        public async Task<CuentaBancaria> ObtenerCuentaAsync(Guid cuentaId)
        {
            var cuenta = new CuentaBancaria(cuentaId, this);

            var eventos = await _repositorioEventos.ObtenerEventosPorAgregadoAsync(cuentaId);

            if (eventos.Any())
            {
                cuenta.ReconstructirDesdeEventos(eventos);
            }

            return cuenta;
        }

        ///// <summary>
        ///// Transfiere dinero de una cuenta a otra
        ///// Se efectúa solamente el evento del dinero enviado, es otro servicio quien recibe el evento y procesa el deposito
        ///// </summary>
        ///// <param name="cuentaOrigenId"></param>
        ///// <param name="cuentaDestinoId"></param>
        ///// <param name="monto"></param>
        ///// <returns></returns>
        //public async Task TransferirAsync(Guid cuentaOrigenId, Guid cuentaDestinoId, decimal monto)
        //{
        //    var cuentaOrigen = await ObtenerCuentaAsync(cuentaOrigenId);

        //    if (cuentaOrigen.Version == 0)
        //        throw new ArgumentException("La cuenta de origen no existe o no ha sido inicializada.");

        //    await cuentaOrigen.TransferirDineroACuentaAsync(cuentaDestinoId, monto);

        //    foreach (var evento in cuentaOrigen.ObtenerEventos())
        //    {
        //        await _repositorioEventos.GuardarEventoAsync(evento);
        //    }
        //    cuentaOrigen.LimpiarEventos();
        //}
    }
}
