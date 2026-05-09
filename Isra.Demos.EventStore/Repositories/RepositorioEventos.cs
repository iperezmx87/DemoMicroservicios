using MongoDB.Driver;

namespace Isra.Demos.EventStore.Repositories
{
    /// <summary>
    /// Implementación del repositorio de eventos usando MongoDB
    /// </summary>
    public class RepositorioEventos : IRepositorioEventos
    {
        private readonly IMongoCollection<EventoBase> _collection;

        /// <summary>
        /// Repositorio de eventos en mongoDB. Este repositorio es responsable de almacenar y recuperar eventos que representan las acciones y cambios de estado en el sistema. Al implementar esta clase, se garantiza que los eventos se gestionen de manera consistente, permitiendo la reconstrucción del estado de los agregados a partir de los eventos almacenados. Además, este repositorio facilita la auditoría y el análisis de los eventos que han ocurrido en el sistema a lo largo del tiempo.
        /// </summary>
        /// <param name="database"></param>
        public RepositorioEventos(IMongoDatabase database)
        {
            _collection = database.GetCollection<EventoBase>(Constantes.EventStoreCollectionName);
        }

        /// <summary>
        /// Guardar el evento en la base de datos. Este método es fundamental para el funcionamiento del Event Sourcing, ya que cada cambio de estado en el sistema se representa como un evento que debe ser almacenado de manera persistente. Al guardar un evento, se asegura que toda la información relevante sobre la acción realizada y su contexto se registre, lo que permite posteriormente reconstruir el estado del sistema a partir de estos eventos. Además, el almacenamiento de eventos facilita la auditoría y el análisis histórico de las acciones realizadas en el sistema.
        /// </summary>
        /// <param name="evento"></param>
        /// <returns></returns>
        public async Task GuardarEventoAsync(EventoBase evento)
        {
            await _collection.InsertOneAsync(evento);
        }

        /// <summary>
        /// Obtiene los eventos por cuenta del agregado. Este método es esencial para la reconstrucción del estado de un agregado a partir de los eventos que lo han afectado. Al recuperar los eventos asociados a un agregado específico, se puede aplicar cada evento en orden para reconstruir el estado actual del agregado. Esto es fundamental en el patrón de Event Sourcing, ya que el estado del sistema no se almacena directamente, sino que se deriva de la secuencia de eventos que han ocurrido a lo largo del tiempo. Además, este método permite analizar el historial de cambios y acciones realizadas sobre un agregado específico.
        /// </summary>
        /// <param name="agregadoId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<EventoBase>> ObtenerEventosPorAgregadoAsync(Guid agregadoId)
        {
            var filter = Builders<EventoBase>.Filter
                .Eq(e => e.AggregateId, agregadoId);

            return await _collection
                .Find(filter)
                .SortBy(e => e.Version)
                .ToListAsync();
        }
    }
}
