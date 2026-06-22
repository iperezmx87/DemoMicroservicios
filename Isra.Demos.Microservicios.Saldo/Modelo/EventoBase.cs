namespace Isra.Demos.Banking.CustomerPosition.Modelo
{
    /// <summary>
    /// Modelo del evento
    /// </summary>
    public abstract class EventoBase
    {
        /// <summary>
        /// Id del evento
        /// </summary>
        public Guid EventId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Id de la cuenta bancaria
        /// </summary>
        public Guid AggregateId { get; set; }

        /// <summary>
        /// Secuencia del evento, para mantener el orden de los eventos en la cuenta bancaria
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Fecha de ocurrido
        /// </summary>
        public DateTime OcurridoEn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Tipo de evento, se obtiene del nombre de la clase que hereda de EventoBase
        /// </summary>
        public string TipoEvento { get; set; }
    }
}
