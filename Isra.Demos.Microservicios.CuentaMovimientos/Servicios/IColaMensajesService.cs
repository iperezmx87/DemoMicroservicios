using Isra.Demos.Microservicios.CuentaMovimientos.Modelo;

namespace Isra.Demos.Microservicios.Servicios
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


        /// <summary>
        /// Publica el evento en la cola de mensajes para envio de transferencia devuelta por error
        /// </summary>
        /// <param name="evento"></param>
        /// <returns></returns>
        Task<Tuple<string, string>> PublicarTransferenciaDevueltaEventoAsync(
            TransferenciaDevueltaEvento evento);
    }
}
