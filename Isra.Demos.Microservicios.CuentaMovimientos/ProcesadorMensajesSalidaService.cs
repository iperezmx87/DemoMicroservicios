using Confluent.Kafka;
using Isra.Demos.Microservicios.CuentaMovimientos.Modelo;
using Isra.Demos.Microservicios.Modelo;
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

        /// <summary>
        /// Constructor del servicio
        /// </summary>
        /// <param name="colaMensajesService"></param>
        /// <param name="mongoDatabase"></param>
        public ProcesadorMensajesSalidaService(
            IColaMensajesService colaMensajesService, 
            IMongoDatabase mongoDatabase)
        {
            _colaMensajesService = colaMensajesService;

            _outboxCollection = mongoDatabase.GetCollection<MensajeSalida>(
                Constantes.CuentasMovimientosCollectionName);
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

                    if (JsonDocument.Parse(msg.Payload).RootElement.GetProperty("TipoEvento").GetString() == "DineroDepositadoEvento")
                    {
                        tplResultPublicar =
                            await _colaMensajesService.PublicarDineroDepositadoEventoAsync(
                                JsonSerializer.Deserialize<DineroDepositadoEvento>(msg.Payload));
                    }
                    else
                    {
                        tplResultPublicar =
                            await _colaMensajesService.PublicarDineroRetiradoEventoAsync(
                                JsonSerializer.Deserialize<DineroRetiradoEvento>(msg.Payload));
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
