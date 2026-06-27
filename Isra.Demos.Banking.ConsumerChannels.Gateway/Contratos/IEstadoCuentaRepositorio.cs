using Isra.Demos.Banking.ConsumerChannels.Gateway.Modelo;

namespace Isra.Demos.Banking.ConsumerChannels.Gateway.Contratos
{
    /// <summary>
    /// Repositorio para obtener el estado de cuenta de una cuenta bancaria, se utiliza para mostrar la información de la cuenta bancaria en el estado de cuenta
    /// </summary>
    public interface IEstadoCuentaRepositorio
    {
        /// <summary>
        /// Obtiene el estado de cuenta de una cuenta bancaria
        /// </summary>
        /// <param name="aggregateId">Id de la cuenta bancaria</param>
        /// <returns>Estado de cuenta de la cuenta bancaria</returns>
        Task<CuentaDto> ObtenerEstadoCuentaAsync(Guid aggregateId);
    }
}
