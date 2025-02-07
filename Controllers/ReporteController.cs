using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.IO.Image;
using iText.Kernel.Geom;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using ProyectoAPI.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iText.Layout.Borders;
using Path = System.IO.Path;

namespace ProyectoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReporteController : ControllerBase
    {
        private readonly CultivoService _cultivoService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ReporteController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly string _pdfDirectory;
        private readonly string _pdfBaseUrl;

        public ReporteController(
            CultivoService cultivoService,
            IConfiguration configuration,
            ILogger<ReporteController> logger,
            IWebHostEnvironment env)
        {
            _cultivoService = cultivoService;
            _configuration = configuration;
            _logger = logger;
            _env = env;
            _pdfDirectory = Path.Combine(env.WebRootPath, "pdfs");
            _pdfBaseUrl = _configuration["PdfBaseUrl"];

            if (!Directory.Exists(_pdfDirectory))
            {
                try
                {
                    Directory.CreateDirectory(_pdfDirectory);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating PDF directory");
                }
            }
        }

        [HttpGet("generarPDF")]
        public async Task<IActionResult> GenerarPDF()
        {
            try
            {
                _logger.LogInformation("Iniciando generación de PDF");

                var datosReporte = await _cultivoService.ObtenerTodosLosDatosCultivoAsync();
                _logger.LogInformation("Datos obtenidos para el reporte");

                string pdfFileName = $"Reporte_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string pdfPath = Path.Combine(_pdfDirectory, pdfFileName);

                using (FileStream fs = new FileStream(pdfPath, FileMode.Create))
                {
                    PdfWriter writer = new PdfWriter(fs);
                    PdfDocument pdf = new PdfDocument(writer);
                    Document document = new Document(pdf, PageSize.A4);

                    AgregarContenidoPDF(document, datosReporte);

                    document.Close();
                }

                _logger.LogInformation("PDF generado exitosamente");

                string pdfUrl = $"{_pdfBaseUrl}/pdfs/{pdfFileName}";

                SchedulePDFDeletion(pdfPath, TimeSpan.FromDays(1));

                return Ok(new
                {
                    Mensaje = $"PDF generado {DateTime.Now:HH:mm:ss}",
                    EnlacePDF = pdfUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF");

                return StatusCode(500, new
                {
                    Mensaje = "Error interno del servidor al generar el PDF",
                    DetallesError = new
                    {
                        Mensaje = ex.Message,
                        TipoExcepcion = ex.GetType().Name,
                        StackTrace = ex.StackTrace
                    }
                });
            }
        }

        private void SchedulePDFDeletion(string pdfPath, TimeSpan delay)
        {
            Task.Run(async () =>
            {
                await Task.Delay(delay);
                try
                {
                    if (System.IO.File.Exists(pdfPath))
                    {
                        System.IO.File.Delete(pdfPath);
                        _logger.LogInformation($"PDF deleted: {pdfPath}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error deleting PDF: {pdfPath}");
                }
            });
        }

        private void AgregarContenidoPDF(Document document, dynamic datosReporte)
        {
            AgregarEncabezado(document);

            AgregarInfoCultivo(document, datosReporte.CultivoActivo, true, datosReporte.DatosIndividuales);

            foreach (var cultivoAnterior in datosReporte.CultivosAnteriores)
            {
                document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                AgregarInfoCultivo(document, cultivoAnterior, false, datosReporte.DatosIndividuales);
            }

            AgregarSeccionImagenes(document, "Fotos tomadas desde la ESP32 CAM", _configuration["ImagenSettings:RutaGuardado"]);
            AgregarSeccionImagenes(document, "Fotos externas al cultivo", _configuration["ImagenSettings:RutaGuardadoExterna"]);

            AgregarPie(document);
        }

        private void AgregarEncabezado(Document document)
        {
            Image logo = new Image(ImageDataFactory.Create("https://i.postimg.cc/15WrTg4z/logo-80-x-80-px.png"));
            logo.SetWidth(50);
            document.Add(logo);

            Paragraph titulo = new Paragraph("Reporte de Cultivos")
                .SetFontSize(24)
                .SetBold()
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(new DeviceRgb(44, 62, 80));
            document.Add(titulo);

            document.Add(new Paragraph("\n"));
        }

        private void AgregarInfoCultivo(Document document, dynamic cultivo, bool esActivo, dynamic todosLosDatos)
        {
            string estado = esActivo ? "ACTIVO" : "ANTERIOR";
            Color bgColor = esActivo ? new DeviceRgb(39, 174, 96) : new DeviceRgb(52, 152, 219);

            if (cultivo == null)
            {
                Paragraph noCultivoInfo = new Paragraph("No hay cultivo activo")
                    .SetFontSize(14)
                    .SetBold()
                    .SetBackgroundColor(new DeviceRgb(231, 76, 60))
                    .SetFontColor(ColorConstants.WHITE)
                    .SetPadding(5);
                document.Add(noCultivoInfo);

                Paragraph mensaje = new Paragraph("Actualmente no hay un cultivo en curso. Inicie un nuevo cultivo para comenzar a recopilar datos.")
                    .SetFontSize(12)
                    .SetMarginTop(10);
                document.Add(mensaje);

                return;
            }

            Paragraph cultivoInfo = new Paragraph($"{cultivo.Nombre} - {estado}")
                .SetFontSize(14)
                .SetBold()
                .SetBackgroundColor(bgColor)
                .SetFontColor(ColorConstants.WHITE)
                .SetPadding(5);
            document.Add(cultivoInfo);

            document.Add(new Paragraph($"Fecha de inicio: {cultivo.FechaInicio}"));
            document.Add(new Paragraph($"Fecha de fin: {cultivo.FechaFin ?? "N/A"}"));
            document.Add(new Paragraph($"Cantidad de datos: {cultivo.CantidadDatos}"));

            AgregarPromedios(document, cultivo);

            var promediosDiarios = CalcularPromediosDiarios(todosLosDatos, (int)cultivo.Id);
            AgregarTablaDatos(document, promediosDiarios);
        }

        private void AgregarPromedios(Document document, dynamic cultivo)
        {
            Paragraph promediosTitulo = new Paragraph("Promedio de la vida del cultivo:")
                .SetFontSize(12)
                .SetBold();
            document.Add(promediosTitulo);

            document.Add(new Paragraph($"Humedad del suelo: {cultivo.PromedioHumedadSuelo:F2}%"));
            document.Add(new Paragraph($"Humedad del aire: {cultivo.PromedioHumedadAire:F2}%"));
            document.Add(new Paragraph($"Temperatura: {cultivo.PromedioTemperatura:F2}°C"));
            document.Add(new Paragraph($"Índice de calor (°C): {cultivo.PromedioIndiceCalorC:F2}°C"));
            document.Add(new Paragraph($"Índice de calor (°F): {cultivo.PromedioIndiceCalorF:F2}°F"));
            document.Add(new Paragraph($"Nivel de agua: {cultivo.PromedioNivelDeAgua:F2}%"));

            document.Add(new Paragraph("\n"));
        }

        private void AgregarTablaDatos(Document document, List<dynamic> promediosDiarios)
        {
            Paragraph tablaTitulo = new Paragraph("Promedios de cada día")
                .SetFontSize(14)
                .SetBold()
                .SetBackgroundColor(new DeviceRgb(52, 152, 219))
                .SetFontColor(ColorConstants.WHITE)
                .SetPadding(5);
            document.Add(tablaTitulo);

            Table table = new Table(new float[] { 2, 1, 1, 1, 1, 1, 1 }).UseAllAvailableWidth();

            string[] headers = { "Fecha", "Hum. Suelo", "Hum. Aire", "Temp.", "Ind. Cal. C", "Ind. Cal. F", "Nivel Agua" };
            foreach (string header in headers)
            {
                table.AddHeaderCell(new Cell().Add(new Paragraph(header).SetBold()).SetBackgroundColor(new DeviceRgb(189, 195, 199)));
            }

            bool alternarColor = false;
            foreach (var promedio in promediosDiarios)
            {
                Color bgColor = alternarColor ? ColorConstants.WHITE : new DeviceRgb(240, 240, 240);
                table.AddCell(new Cell().Add(new Paragraph(promedio.Fecha.ToString("yyyy-MM-dd"))).SetBackgroundColor(bgColor));
                table.AddCell(new Cell().Add(new Paragraph($"{promedio.PromedioHumedadSuelo:F2}%")).SetBackgroundColor(bgColor));
                table.AddCell(new Cell().Add(new Paragraph($"{promedio.PromedioHumedadAire:F2}%")).SetBackgroundColor(bgColor));
                table.AddCell(new Cell().Add(new Paragraph($"{promedio.PromedioTemperatura:F2}°C")).SetBackgroundColor(bgColor));
                table.AddCell(new Cell().Add(new Paragraph($"{promedio.PromedioIndiceCalorC:F2}°C")).SetBackgroundColor(bgColor));
                table.AddCell(new Cell().Add(new Paragraph($"{promedio.PromedioIndiceCalorF:F2}°F")).SetBackgroundColor(bgColor));
                table.AddCell(new Cell().Add(new Paragraph($"{promedio.PromedioNivelDeAgua:F2}%")).SetBackgroundColor(bgColor));
                alternarColor = !alternarColor;
            }

            document.Add(table);
        }

        private List<dynamic> CalcularPromediosDiarios(dynamic todosLosDatos, int cultivoId)
        {
            var promediosDiarios = new List<dynamic>();
            var datosPorDia = new Dictionary<DateTime, List<dynamic>>();

            foreach (var dato in todosLosDatos)
            {
                if ((int)dato.Id == cultivoId)
                {
                    var fecha = ((DateTime)dato.FechaHora).Date;
                    if (!datosPorDia.ContainsKey(fecha))
                    {
                        datosPorDia[fecha] = new List<dynamic>();
                    }
                    datosPorDia[fecha].Add(dato);
                }
            }

            foreach (var kvp in datosPorDia.OrderByDescending(x => x.Key).Take(31))
            {
                var fecha = kvp.Key;
                var datosDia = kvp.Value;

                var promedioDia = new
                {
                    Fecha = fecha,
                    PromedioHumedadSuelo = datosDia.Average(d => (float)d.HumedadSuelo),
                    PromedioHumedadAire = datosDia.Average(d => (float)d.HumedadAire),
                    PromedioTemperatura = datosDia.Average(d => (float)d.Temperatura),
                    PromedioIndiceCalorC = datosDia.Average(d => (float)d.indiceCalorC),
                    PromedioIndiceCalorF = datosDia.Average(d => (float)d.indiceCalorF),
                    PromedioNivelDeAgua = datosDia.Average(d => (float)d.NivelDeAgua)
                };

                promediosDiarios.Add(promedioDia);
            }

            return promediosDiarios;
        }

        private void AgregarSeccionImagenes(Document document, string titulo, string rutaImagenes)
        {
            document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

            Paragraph tituloImagenes = new Paragraph(titulo)
                .SetFontSize(16)
                .SetBold()
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(new DeviceRgb(52, 152, 219));
            document.Add(tituloImagenes);

            var archivosImagen = Directory.GetFiles(rutaImagenes, "*.jpg");

            Table tablaImagenes = new Table(UnitValue.CreatePercentArray(5)).UseAllAvailableWidth();

            foreach (var archivo in archivosImagen)
            {
                Image imagen = new Image(ImageDataFactory.Create(archivo));
                imagen.ScaleToFit(80, 80);
                tablaImagenes.AddCell(new Cell().Add(imagen).SetBorder(Border.NO_BORDER));
            }

            int celdasFaltantes = 5 - (archivosImagen.Length % 5);
            if (celdasFaltantes < 5)
            {
                for (int i = 0; i < celdasFaltantes; i++)
                {
                    tablaImagenes.AddCell(new Cell().Add(new Paragraph("")).SetBorder(Border.NO_BORDER));
                }
            }

            document.Add(tablaImagenes);
        }

        private void AgregarPie(Document document)
        {
            Paragraph footer = new Paragraph("Reporte generado automáticamente por el sistema de monitoreo de cultivos.")
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(ColorConstants.GRAY);

            Div footerDiv = new Div()
                .SetFixedPosition(0, 20, PageSize.A4.GetWidth())
                .SetTextAlignment(TextAlignment.CENTER)
                .Add(footer);

            document.Add(footerDiv);
        }
    }
}