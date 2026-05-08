namespace Isra.Demos.EventStore.Services
{
    /// <summary>
    /// Servicio para manejar operaciones en cuentas bancarias
    /// </summary>
    public interface ICuentaBancariaService
    {
        /// <summary>
        /// Crea la cuenta bancaria. Este método es responsable de inicializar una nueva cuenta bancaria en el sistema. Al crear una cuenta, se establece un identificador único para la cuenta y se prepara para recibir eventos relacionados con las operaciones que se realizarán en ella, como depósitos y retiros. La creación de la cuenta es el primer paso para permitir a los usuarios interactuar con sus cuentas banc
        /// </summary>
        /// <param name="cuentaId"></param>
        /// <returns></returns>
        Task CrearCuentaAsync(Guid cuentaId);

        /// <summary>
        /// Deposita dinero en la cuenta
        /// </summary>
        /// <param name="cuentaId"></param>
        /// <param name="monto"></param>
        /// <param name="propietario"></param>
        /// <returns></returns>
        Task DepositarAsync(Guid cuentaId, decimal monto, string propietario);

        /// <summary>
        /// Retira dinero de la cuenta. Este método es responsable de manejar las operaciones de retiro en una cuenta bancaria. Al retirar dinero, se verifica que la cuenta tenga fondos suficientes para cubrir el monto del retiro y se generan los eventos correspondientes para reflejar esta acción en el sistema. El retiro de dinero es una operación crítica que afecta el saldo de la cuenta, por lo que es importante manejarlo con cuidado para garantizar la integridad de los datos y la correcta gestión de los eventos relacionados con esta acción.
        /// </summary>
        /// <param name="cuentaId"></param>
        /// <param name="monto"></param>
        /// <param name="propietario"></param>
        /// <returns></returns>
        Task RetirarAsync(Guid cuentaId, decimal monto, string propietario);

        /// <summary>
        /// Obtiene la información de la cuenta bancaria. Este método es responsable de recuperar la información actualizada de una cuenta bancaria, incluyendo su saldo y el historial de eventos asociados a esa cuenta. Al obtener la información de la cuenta, se reconstruye el estado actual de la cuenta a partir de los eventos registrados, lo que permite a los usuarios ver el saldo actual y las transacciones realizadas en la cuenta. Esta funcionalidad es esencial para proporcionar a los usuarios una visión clara y precisa de su situación financiera en cualquier momento dado.
        /// </summary>
        /// <param name="cuentaId"></param>
        /// <returns></returns>
        Task<CuentaBancaria> ObtenerCuentaAsync(Guid cuentaId);
    }
}
