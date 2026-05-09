namespace Isra.Demos.EventStore.Services
{
    /// <summary>
    /// Cola de mensajes
    /// </summary>
    public interface IColaMensajesService
    {
        /// <summary>
        /// Publica el evento en la cola de mensajes
        /// </summary>
        /// <param name="evento"></param>
        /// <returns></returns>
        Task PublicarDineroDepositadoEventoAsync(
            DineroDepositadoEvento evento);

        /// <summary>
        /// Publica el evento en la cola de mensajes
        /// </summary>
        /// <param name="evento"></param>
        /// <returns></returns>
        Task PublicarDineroRetiradoEventoAsync(
            DineroRetiradoEvento evento);
    }
}
