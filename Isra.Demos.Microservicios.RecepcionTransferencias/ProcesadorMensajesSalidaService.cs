using Confluent.Kafka;
using Isra.Demos.Microservicios.RecepcionTransferencias.Modelo;
using Isra.Demos.Microservicios.RecepcionTransferencias.Servicios;
using MongoDB.Driver;
using System.Text.Json;

namespace Isra.Demos.Microservicios.RecepcionTransferencias
{
    /// <summary>
    /// Procesador de los mensajes de salida de mongodb que los publica en kafka
    /// </summary>
    public class ProcesadorMensajesSalidaService
        : BackgroundService
    {
        private readonly IColaMensajesService _colaMensajesService;
        private readonly IMongoCollection<MensajeSalida> _outboxCollection;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor del servicio
        /// </summary>
        /// <param name="colaMensajesService"></param>
        /// <param name="mongoDatabase"></param>
        /// <param name="configuration"></param>
        public ProcesadorMensajesSalidaService(
            IColaMensajesService colaMensajesService,
            IMongoDatabase mongoDatabase,
            IConfiguration configuration)
        {
            _configuration = configuration;
            _colaMensajesService = colaMensajesService;

            _outboxCollection = mongoDatabase.GetCollection<MensajeSalida>(
                _configuration.GetValue<string>("MongoDb:CuentasMovimientosOutboxCollectionName"));
        }

        /// <summary>
        /// Procesa los mensajes de salida
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var mensajesPendientes = await _outboxCollection
                    .Find(m => !m.Processed)
                    .Limit(10)
                    .ToListAsync();

                foreach (var msg in mensajesPendientes)
                {
                    Tuple<string, string> tplResultPublicar = new Tuple<string, string>("", "");

                    switch (JsonDocument.Parse(msg.Payload).RootElement.GetProperty("TipoEvento").GetString())
                    {
                        case "TransferenciaRecibidaEvento":
                            tplResultPublicar = await _colaMensajesService.PublicarTransferenciaRecibidaEventoAsync(
                                JsonSerializer.Deserialize<TransferenciaRecibidaEvento>(msg.Payload));
                            break;

                        default:
                            break;
                    }

                    if (tplResultPublicar.Item2 == nameof(PersistenceStatus.Persisted))
                    {
                        await _outboxCollection.UpdateOneAsync(
                            m => m.Id == msg.Id,
                            Builders<MensajeSalida>.Update.Set(m => m.Processed, true)
                        );
                    }
                }

                await Task.Delay(10000); // Esperar diez segundos antes de la siguiente vuelta
            }
        }
    }
}
