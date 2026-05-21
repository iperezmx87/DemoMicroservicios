
using Isra.Demos.Microservicios.RecepcionTransferencias.Modelo;

namespace Isra.Demos.Microservicios.RecepcionTransferencias.Servicios
{
    /// <summary>
    /// Servicio para manejar operaciones en cuentas bancarias
    /// </summary>
    public interface ICuentaBancariaService
    {
        /// <summary>
        /// Obtiene la información de la cuenta bancaria. Este método es responsable de recuperar la información actualizada de una cuenta bancaria, incluyendo su saldo y el historial de eventos asociados a esa cuenta. Al obtener la información de la cuenta, se reconstruye el estado actual de la cuenta a partir de los eventos registrados, lo que permite a los usuarios ver el saldo actual y las transacciones realizadas en la cuenta. Esta funcionalidad es esencial para proporcionar a los usuarios una visión clara y precisa de su situación financiera en cualquier momento dado.
        /// </summary>
        /// <param name="cuentaId"></param>
        /// <returns></returns>
        Task<CuentaBancaria> ObtenerCuentaAsync(Guid cuentaId);

        /// <summary>
        /// Transfiere dinero de una cuenta origen a una cuenta destino.
        /// </summary>
        /// <param name="cuentaDestinoId"></param>
        /// <param name="monto"></param>
        /// <returns></returns>
        Task RecibirTransferenciaAsync(Guid cuentaDestinoId, decimal monto);
    }
}
