using Isra.Demos.Banking.PaymentExecution.Modelo;

namespace Isra.Demos.Banking.PaymentExecution.Servicios
{
    /// <summary>
    /// Cola de mensajes
    /// </summary>
    public interface IColaMensajesService
    {
        /// <summary>
        /// Publica el evento en la cola de mensajes para envio de transferencia recibida
        /// </summary>
        /// <param name="evento"></param>
        /// <returns></returns>
        Task<Tuple<string,string>> PublicarTransferenciaRecibidaEventoAsync(
            TransferenciaRecibidaEvento evento);

        /// <summary>
        /// Publica el evento en la cola de mensajes para envio de transferencia devuelta por error
        /// </summary>
        /// <param name="evento"></param>
        /// <returns></returns>
        Task<Tuple<string, string>> PublicarTransferenciaDevueltaEventoAsync(
            TransferenciaDevueltaEvento evento);
    }
}
