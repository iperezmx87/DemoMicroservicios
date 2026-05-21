using Isra.Demos.Microservicios.WebApi.Contratos;
using Isra.Demos.Microservicios.WebApi.Modelo;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace Isra.Demos.Microservicios.WebApi.Servicios
{
    /// <summary>
    /// Generador de estados de cuenta pdf con QuestPDF
    /// </summary>
    public class GeneradorEstadoCuentaPdfService
        : IGeneradorEstadoCuentaPdfService
    {
        private readonly IEstadoCuentaRepositorio _estadoCuentaRepositorio;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="estadoCuentaRepositorio"></param>
        public GeneradorEstadoCuentaPdfService(IEstadoCuentaRepositorio estadoCuentaRepositorio)
        {
            _estadoCuentaRepositorio = estadoCuentaRepositorio;
        }

        /// <summary>
        /// Generando el estado de cuenta en pdf
        /// </summary>
        /// <param name="idCuenta"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<byte[]> GenerarEstadoCuentaPdf(Guid idCuenta)
        {
            var cultura = new CultureInfo("es-MX");
            CultureInfo.DefaultThreadCurrentCulture = cultura;
            CultureInfo.DefaultThreadCurrentUICulture = cultura;

            // Nota: QuestPDF requiere configurar la licencia (la comunitaria es gratis)
            QuestPDF.Settings.License = LicenseType.Community;

            CuentaDto cuenta = await _estadoCuentaRepositorio.ObtenerEstadoCuentaAsync(idCuenta);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Size(PageSizes.Letter);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.SegoeUI));

                    // --- CABECERA ---
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Banquito de isra").FontSize(24).SemiBold().FontColor(Colors.Indigo.Medium);
                        });

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("ESTADO DE CUENTA").FontSize(14).SemiBold().FontColor(Colors.Grey.Medium).AlignRight();
                            col.Item().Text($"Fecha: {DateTime.Now:dd/MM/yyyy}").AlignRight();
                        });
                    });

                    // --- CONTENIDO ---
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        // Cuadro de información
                        col.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(c =>
                        {
                            c.Item().Text($"Propietario: {cuenta.Propietario}").SemiBold();
                            c.Item().Text($"ID Cuenta: {cuenta.AggregateId}");
                        });

                        col.Item().PaddingTop(20).Table(table =>
                        {
                            // Definir columnas
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2); // Fecha
                                columns.RelativeColumn(3); // Descripción
                                columns.RelativeColumn(2); // Monto
                            });

                            // Encabezado de tabla
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("FECHA");
                                header.Cell().Element(CellStyle).Text("TIPO DE MOVIMIENTO");
                                header.Cell().Element(CellStyle).AlignRight().Text("MONTO");

                                static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                            });

                            // Filas de movimientos
                            foreach (var m in cuenta.Movimientos)
                            {
                                table.Cell().Element(RowStyle).Text(m.FechaEvento.ToString("dd/MM/yyyy HH:mm"));
                                table.Cell().Element(RowStyle).Text(m.TipoMovimiento);

                                var montoTexto = m.Monto.ToString("C");

                                table.Cell().Element(RowStyle).AlignRight().Text(montoTexto)
                                     .FontColor(m.TipoMovimiento == "Deposito" || m.TipoMovimiento == "Devolución de transferencia" ? Colors.Green.Medium : Colors.Red.Medium);

                                static IContainer RowStyle(IContainer container) => container.PaddingVertical(3).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                            }
                        });

                        col.Item().AlignRight().PaddingTop(20).Text(t =>
                        {
                            t.Span("SALDO TOTAL: ").FontSize(12);
                            t.Span(cuenta.Saldo.ToString("C")).FontSize(16).Bold().FontColor(Colors.Indigo.Medium);
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                    });
                });
            }).GeneratePdf();
        }
    }
}
