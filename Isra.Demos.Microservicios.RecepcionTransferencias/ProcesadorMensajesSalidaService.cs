using Confluent.Kafka;
using Isra.Demos.Microservicios.RecepcionTransferencias.Monitoreo;
using Isra.Demos.Microservicios.RecepcionTransferencias.Modelo;
using Isra.Demos.Microservicios.RecepcionTransferencias.Servicios;
using MongoDB.Driver;
using System.Diagnostics;
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
                    // 2. Reconstruimos el contexto de la traza original almacenado en el documento de Mongo
                    var parentContext = string.IsNullOrEmpty(msg.TraceId)
                        ? default
                        : ActivityContext.Parse(msg.TraceId, null);

                    // 3. Iniciamos un Span de tipo Producer que enlaza al flujo original del API
                    using Activity activity = MicroservicioTelemetry.Source.StartActivity(
                        "Outbox Relay - Receptor transferencias: Publicar a Kafka",
                        ActivityKind.Producer,
                        parentContext);

                    var tipoEvento = JsonDocument.Parse(msg.Payload).RootElement.GetProperty("TipoEvento").GetString();

                    // Enriquecemos la traza con metadata útil para el diagnóstico corporativo
                    if (activity != null)
                    {
                        activity.SetTag("messaging.system", "kafka");
                        activity.SetTag("messaging.operation", "publish");
                        activity.SetTag("banco.evento.tipo", tipoEvento);
                        activity.SetTag("outbox.mensaje.id", msg.Id.ToString());
                    }

                    Tuple<string, string> tplResultPublicar = new Tuple<string, string>("", "");

                    try
                    {

                        switch (JsonDocument.Parse(msg.Payload).RootElement.GetProperty("TipoEvento").GetString())
                        {
                            case "TransferenciaRecibidaEvento":
                                tplResultPublicar = await _colaMensajesService.PublicarTransferenciaRecibidaEventoAsync(
                                    JsonSerializer.Deserialize<TransferenciaRecibidaEvento>(msg.Payload));
                                break;

                            case "TransferenciaDevueltaEvento":
                                tplResultPublicar = await _colaMensajesService.PublicarTransferenciaDevueltaEventoAsync(
                                    JsonSerializer.Deserialize<TransferenciaDevueltaEvento>(msg.Payload));
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

                            activity?.SetStatus(ActivityStatusCode.Ok);
                        }
                    }
                    catch (Exception ex)
                    {
                        // 4. Si el relay truena (por ejemplo, Kafka caído), registramos el fallo en OTel
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                        activity?.AddException(ex);
                        throw;
                    }
                }

                await Task.Delay(10000);
            }
        }
    }
}
