using Isra.Demos.Microservicios.RecepcionTransferencias.Servicios;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Isra.Demos.Microservicios.RecepcionTransferencias.Modelo
{
    /// <summary>
    /// Agregado que representa una cuenta bancaria y su estado
    /// </summary>
    public class CuentaBancaria
    {
        private readonly ICuentaBancariaService _cuentaBancariaService;

        /// <summary>
        /// Id de la cuenta bancaria, se utiliza como AggregateId para los eventos relacionados con esta cuenta. Este identificador es fundamental para el patrón de Event Sourcing, ya que permite asociar todos los eventos que afectan a esta cuenta específica. Al utilizar un Guid como identificador, se garantiza la unicidad de cada cuenta bancaria en el sistema, lo que facilita la gestión y recuperación de eventos relacionados con esta cuenta en particular. Además, este Id es esencial para reconstruir el estado de la cuenta a partir de los eventos almacenados en el repositorio de eventos.
        /// </summary>
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid Id { get; private set; }

        /// <summary>
        /// Saldo actual de la cuenta bancaria. Este campo representa el monto de dinero disponible en la cuenta en un momento dado. El saldo se actualiza cada vez que se realiza una operación de depósito o retiro, reflejando así el estado financiero actual de la cuenta. Es fundamental para la gestión de la cuenta y para garantizar que las operaciones se realicen de manera correcta, evitando sobregiros o transacciones no autorizadas. Además, el saldo es un indicador clave para los titulares de la cuenta y para los sistemas que gestionan las finanzas personales o empresariales.
        /// </summary>
        public decimal Saldo { get; private set; }

        /// <summary>
        /// Version del evento
        /// </summary>
        public int Version { get; private set; }

        /// <summary>
        /// Lista de eventos que han ocurrido de dinero depositado o retirado, pero que aún no han sido persistidos en el repositorio de eventos. Esta lista es fundamental para el patrón de Event Sourcing, ya que permite acumular los eventos generados por las operaciones realizadas en la cuenta bancaria antes de guardarlos de manera persistente. Al mantener esta lista de eventos sin aplicar, se facilita
        /// </summary>
        private readonly List<EventoBase> _eventos = new();

        /// <summary>
        /// Constructor para crear una nueva cuenta bancaria con un Id específico. Este constructor es esencial para inicializar una cuenta bancaria con un identificador único, lo que permite asociar los eventos relacionados con esta cuenta de manera consistente. Al establecer el saldo inicial en 0 y la versión en 0, se garantiza que la cuenta comience con un estado limpio, listo para recibir eventos de depósito y retiro que modificarán su estado a lo largo del tiempo. Además, este constructor facilita la creación de cuentas bancarias a partir de eventos históricos, permitiendo reconstruir el estado de la cuenta a partir de los eventos almacenados en el repositorio de eventos.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cuentaBancariaService"></param>
        public CuentaBancaria(Guid id, ICuentaBancariaService cuentaBancariaService)
        {
            Id = id;
            Saldo = 0;
            Version = 0;
            _cuentaBancariaService = cuentaBancariaService;
        }

        /// <summary>
        /// Obtiene los eventos sin aplicados
        /// </summary>
        public IReadOnlyList<EventoBase> ObtenerEventos() => _eventos.AsReadOnly();

        /// <summary>
        /// Limpia los eventos después de persistirlos
        /// </summary>
        public void LimpiarEventos() => _eventos.Clear();

        /// <summary>
        /// Procesar la transferencia
        /// </summary>
        /// <param name="idCuentaDestino"></param>
        /// <param name="monto"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task RecibirTransferenciaAsync(Guid idCuentaDestino, decimal monto)
        {
            var cuentaDestino = await _cuentaBancariaService.ObtenerCuentaAsync(idCuentaDestino);

            if (cuentaDestino == null)
                throw new ArgumentException("La cuenta destino no existe", nameof(idCuentaDestino));

            if (monto <= 0)
                throw new ArgumentException("El monto debe ser mayor a 0", nameof(monto));

            // validar si la cuenta es correcta
            var evento = new TransferenciaRecibidaEvento(idCuentaDestino, monto, Version + 1);

            AplicarEvento(evento);

            _eventos.Add(evento);
        }

        /// <summary>
        /// Reconstruir el estado a partir de eventos históricos
        /// </summary>
        public void ReconstructirDesdeEventos(IEnumerable<EventoBase> eventos)
        {
            foreach (var evento in eventos.OrderBy(e => e.Version))
            {
                AplicarEvento(evento);
            }
        }

        /// <summary>
        /// Aplica un evento al estado actual
        /// </summary>
        private void AplicarEvento(EventoBase evento)
        {
            switch (evento)
            {
                case DineroDepositadoEvento dineroDepositado:
                    Saldo += dineroDepositado.Monto;
                    Version = dineroDepositado.Version;
                    break;

                case DineroRetiradoEvento dineroRetirado:
                    Saldo -= dineroRetirado.Monto;
                    Version = dineroRetirado.Version;
                    break;

                case TransferenciaRealizadaEvento transferenciaRealizada:
                    Saldo -= transferenciaRealizada.Monto;
                    Version = transferenciaRealizada.Version;
                    break;

                case TransferenciaRecibidaEvento transferenciaRecibida:
                    Saldo += transferenciaRecibida.Monto;
                    Version = transferenciaRecibida.Version;
                    break;

                case TransferenciaDevueltaEvento transferenciaDevuelta:
                    Saldo -= transferenciaDevuelta.Monto;
                    Version = transferenciaDevuelta.Version;
                    break;
            }
        }
    }
}
