using Isra.Demos.Microservicios.CuentaMovimientos.Modelo;

namespace Isra.Demos.Microservicios.Servicios
{
    /// <summary>
    /// Servicio para manejar operaciones en cuentas bancarias
    /// </summary>
    public interface ICuentaBancariaService
    {
        /// <summary>
        /// Deposita dinero en la cuenta
        /// </summary>
        /// <param name="cuentaId"></param>
        /// <param name="monto"></param>
        /// <returns></returns>
        Task DepositarAsync(Guid cuentaId, decimal monto);

        /// <summary>
        /// Retira dinero de la cuenta. Este método es responsable de manejar las operaciones de retiro en una cuenta bancaria. Al retirar dinero, se verifica que la cuenta tenga fondos suficientes para cubrir el monto del retiro y se generan los eventos correspondientes para reflejar esta acción en el sistema. El retiro de dinero es una operación crítica que afecta el saldo de la cuenta, por lo que es importante manejarlo con cuidado para garantizar la integridad de los datos y la correcta gestión de los eventos relacionados con esta acción.
        /// </summary>
        /// <param name="cuentaId"></param>
        /// <param name="monto"></param>
        /// <returns></returns>
        Task RetirarAsync(Guid cuentaId, decimal monto);

        /// <summary>
        /// Obtiene la información de la cuenta bancaria. Este método es responsable de recuperar la información actualizada de una cuenta bancaria, incluyendo su saldo y el historial de eventos asociados a esa cuenta. Al obtener la información de la cuenta, se reconstruye el estado actual de la cuenta a partir de los eventos registrados, lo que permite a los usuarios ver el saldo actual y las transacciones realizadas en la cuenta. Esta funcionalidad es esencial para proporcionar a los usuarios una visión clara y precisa de su situación financiera en cualquier momento dado.
        /// </summary>
        /// <param name="cuentaId"></param>
        /// <returns></returns>
        Task<CuentaBancaria> ObtenerCuentaAsync(Guid cuentaId);

        /// <summary>
        /// Transfiere dinero de una cuenta origen a una cuenta destino.
        /// </summary>
        /// <param name="cuentaOrigenId"></param>
        /// <param name="cuentaDestinoId"></param>
        /// <param name="monto"></param>
        /// <returns></returns>
        Task TransferirAsync(Guid cuentaOrigenId, Guid cuentaDestinoId, decimal monto);
    }
}
