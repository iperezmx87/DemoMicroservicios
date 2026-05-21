using Confluent.Kafka;
using Dapper;
using Isra.Demos.Microservicios.Saldo.Modelo;
using Npgsql;
using System.Text.Json;

namespace Isra.Demos.Microservicios.Saldo
{
    /// <summary>
    /// worker que se ejecuta en segundo plano y realiza tareas periódicas, como registrar información sobre su ejecución. En este caso, el worker registra un mensaje cada segundo indicando que está en funcionamiento, lo que puede ser útil para monitorear su estado y asegurarse de que está activo. El uso de un BackgroundService permite que el worker se ejecute de manera asíncrona y se detenga de forma ordenada cuando se solicite la cancelación, lo que es esencial para garantizar la estabilidad y el rendimiento de la aplicación en la que se integra este worker.
    /// </summary>
    public class SaldoConsumerService : BackgroundService
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        /// <summary>
        /// Constructor
        /// </summary>
        public SaldoConsumerService(IConfiguration configuration)
        {
            _configuration = configuration;

            var config = new ConsumerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"],
                GroupId = "cuenta-saldo-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<Ignore, string>(config).Build();

            _connectionString = _configuration["ConnectionStrings:PostgresSaldoConnection"];
        }

        /// <summary>
        /// Ejecutar el servicio
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                _consumer.Subscribe(_configuration["Kafka:Topic"]);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = _consumer.Consume(stoppingToken);
                        var eventoJson = result.Message.Value;

                        // 1. Analizamos el JSON sin deserializarlo a clase todavía
                        using JsonDocument doc = JsonDocument.Parse(eventoJson);
                        JsonElement root = doc.RootElement;

                        // 2. Buscamos la propiedad que diferencia el evento (ej. "TipoEvento")
                        string tipoEvento = root.GetProperty("TipoEvento").GetString();

                        switch (tipoEvento)
                        {
                            case "DineroDepositadoEvento":
                                var deposito = JsonSerializer.Deserialize<DineroDepositadoEvento>(eventoJson);

                                await ActualizarSaldo(deposito, esDeposito: true);

                                Console.WriteLine($"Deposito: {deposito.AggregateId} {deposito.Monto}");

                                break;

                            case "DineroRetiradoEvento":
                                var retiro = JsonSerializer.Deserialize<DineroRetiradoEvento>(eventoJson);

                                await ActualizarSaldo(retiro, esDeposito: false);

                                Console.WriteLine($"Retiro: {retiro.AggregateId} {retiro.Monto}");

                                break;

                            case "TransferenciaRealizadaEvento":
                                var transferenciaEnviada = JsonSerializer.Deserialize<TransferenciaRealizadaEvento>(eventoJson);

                                await ActualizarSaldo(transferenciaEnviada, esDeposito: false);

                                Console.WriteLine($"Retiro por transferencia: {transferenciaEnviada.AggregateId} {transferenciaEnviada.Monto}");

                                break;

                            case "TransferenciaRecibidaEvento":
                                var transferenciaRecibida = JsonSerializer.Deserialize<TransferenciaRecibidaEvento>(eventoJson);

                                await ActualizarSaldo(transferenciaRecibida, esDeposito: true);

                                Console.WriteLine($"Deposito por transferencia: {transferenciaRecibida.AggregateId} {transferenciaRecibida.Monto}");

                                break;

                            case "TransferenciaDevueltaEvento":
                                var transferenciaDevuelta = JsonSerializer.Deserialize<TransferenciaDevueltaEvento>(eventoJson);

                                await ActualizarSaldo(transferenciaDevuelta, esDeposito: true);

                                Console.WriteLine($"Deposito por devolución: {transferenciaDevuelta.AggregateId} {transferenciaDevuelta.Monto}");

                                break;
                            default:
                                break;
                        }
                    }
                    catch (OperationCanceledException) { break; }
                    catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
                }

            }, stoppingToken);
        }

        private async Task ActualizarSaldo(EventoBase evento, bool esDeposito)
        {
            using var conn = new NpgsqlConnection(_connectionString);

            await conn.OpenAsync();

            // El monto depende de si es deposito o retiro
            decimal montoModificador = 0;

            if (evento is DineroDepositadoEvento d) { montoModificador = d.Monto; }
            if (evento is DineroRetiradoEvento r) { montoModificador = -r.Monto; }
            if (evento is TransferenciaRealizadaEvento t) { montoModificador = -t.Monto; }
            if (evento is TransferenciaDevueltaEvento td) { montoModificador = td.Monto; }
            if (evento is TransferenciaRecibidaEvento tr) { montoModificador = tr.Monto; }

            // SQL Upsert con validación de versión
            // Solo actualiza si la versión del evento es mayor a la que tenemos
            string sql = @"
            INSERT INTO cuentas.saldos_cuenta (id, saldo, ultima_version)
            VALUES (@Id, @Monto, @Version)
            ON CONFLICT (id) DO UPDATE 
            SET saldo = cuentas.saldos_cuenta.saldo + EXCLUDED.saldo,
                ultima_version = EXCLUDED.ultima_version,
                ultima_actualizacion = CURRENT_TIMESTAMP
            WHERE cuentas.saldos_cuenta.ultima_version < EXCLUDED.ultima_version;";

            await conn.ExecuteAsync(sql, new
            {
                Id = evento.AggregateId,
                Monto = montoModificador,
                evento.Version
            });
        }
    }
}
