using Isra.Demos.Microservicios.Modelo;

namespace Isra.Demos.Microservicios.CuentaMovimientos.Repositorio
{
    /// <summary>
    /// Repositorio de eventos para un sistema basado en Event Sourcing. Este repositorio es responsable de almacenar y recuperar eventos que representan las acciones y cambios de estado en el sistema. Al implementar esta interfaz, se garantiza que los eventos se gestionen de manera consistente, permitiendo la reconstrucción del estado de los agregados a partir de los eventos almacenados. Además, este repositorio facilita la auditoría y el análisis de los eventos que han ocurrido en el sistema a lo largo del tiempo.
    /// </summary>
    public interface IRepositorioEventos
    {
        /// <summary>
        /// Almacena un evento en el repositorio de eventos. Este método es fundamental para la persistencia de eventos en un sistema basado en Event Sourcing. Al guardar un evento, se asegura que todas las acciones que han ocurrido en el sistema queden registradas y puedan ser reconstruidas posteriormente para obtener el estado actual del agregado o para realizar auditorías.
        /// </summary>
        /// <param name="evento"></param>
        /// <returns></returns>
        Task GuardarEventoAsync(EventoBase evento);

        /// <summary>
        /// Obtiene los eventos por cuenta bancaria
        /// </summary>
        /// <param name="agregadoId"></param>
        /// <returns></returns>
        Task<IEnumerable<EventoBase>> ObtenerEventosPorAgregadoAsync(Guid agregadoId);
    }
}
