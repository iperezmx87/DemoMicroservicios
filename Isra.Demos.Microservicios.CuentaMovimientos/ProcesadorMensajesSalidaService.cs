using Confluent.Kafka;
using Isra.Demos.Microservicios.CuentaMovimientos.Modelo;
using Isra.Demos.Microservicios.Servicios;
using System.Text.Json;

namespace Isra.Demos.Microservicios.CuentaMovimientos
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
                    Tuple<string, string> tplResultPublicar;

                    switch (JsonDocument.Parse(msg.Payload).RootElement.GetProperty("TipoEvento").GetString())
                    {
                        case "DineroDepositadoEvento":
                            tplResultPublicar =
                                await _colaMensajesService.PublicarDineroDepositadoEventoAsync(
                                    JsonSerializer.Deserialize<DineroDepositadoEvento>(msg.Payload));
                            break;
                        case "DineroRetiradoEvento":
                            tplResultPublicar =
                                await _colaMensajesService.PublicarDineroRetiradoEventoAsync(
                                    JsonSerializer.Deserialize<DineroRetiradoEvento>(msg.Payload));
                            break;

                        case "TransferenciaDevueltaEvento":
                            tplResultPublicar = await _colaMensajesService.PublicarTransferenciaDevueltaEventoAsync(
                                JsonSerializer.Deserialize<TransferenciaDevueltaEvento>(msg.Payload));
                            break;

                        case "TransferenciaRealizadaEvento":
                            tplResultPublicar = await _colaMensajesService.PublicarTransferenciaRealizadaEventoAsync(
                                JsonSerializer.Deserialize<TransferenciaRealizadaEvento>(msg.Payload));
                            break;

                        default:
                            throw new InvalidOperationException("Tipo de evento desconocido en el mensaje de salida.");
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
