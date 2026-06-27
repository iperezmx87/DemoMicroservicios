using Isra.Demos.Banking.CurrentAccount.Modelo;

namespace Isra.Demos.Banking.CurrentAccount.Servicios
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
        Task<Tuple<string, string>> PublicarDineroDepositadoEventoAsync(
            DineroDepositadoEvento evento);

        /// <summary>
        /// Publica el evento en la cola de mensajes
        /// </summary>
        /// <param name="evento"></param>
        /// <returns></returns>
        Task<Tuple<string, string>> PublicarDineroRetiradoEventoAsync(
            DineroRetiradoEvento evento);

        /// <summary>
        /// Publica el evento en la cola de mensajes para envio de transferencia realizada
        /// </summary>
        /// <param name="evento"></param>
        /// <returns></returns>
        Task<Tuple<string, string>> PublicarTransferenciaRealizadaEventoAsync(
            TransferenciaRealizadaEvento evento);
    }
}
