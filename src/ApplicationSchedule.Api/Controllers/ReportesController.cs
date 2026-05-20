using ApplicationSchedule.Application.DTOs.Asignaciones;
using ApplicationSchedule.Application.DTOs.Horarios;
using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ApplicationSchedule.Application.Security;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ApplicationSchedule.Api.Controllers;
[Authorize(Roles = RolesSistema.AdministradorOCoordinador)]
[ApiController]
[Route("api/reportes")]
public class ReportesController : ControllerBase
{
    private readonly IReporteCargaService _reporteCargaService;
    private readonly IConflictoAsignacionService _conflictoService;
    private readonly AppDbContext _context;

    public ReportesController(
        IReporteCargaService reporteCargaService,
        IConflictoAsignacionService conflictoService,
        AppDbContext context)
    {
        _reporteCargaService = reporteCargaService;
        _conflictoService = conflictoService;
        _context = context;
    }

    /// <summary>
    /// Genera el reporte de horas asignadas vs carga contractual
    /// para todos los docentes en un semestre.
    /// </summary>
    [HttpGet("carga-docente")]
    public async Task<ActionResult<ReporteCargaDocenteResponse>> ObtenerReporteCarga(
        [FromQuery] string semestre,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(semestre))
            return BadRequest(new { mensaje = "El parámetro 'semestre' es obligatorio." });

        ReporteCargaDocenteResponse reporte =
            await _reporteCargaService.GenerarReportePorSemestreAsync(semestre, cancellationToken);

        return Ok(reporte);
    }

    /// <summary>
    /// Genera el reporte de horas asignadas vs carga contractual
    /// para un docente específico en un semestre.
    /// </summary>
    [HttpGet("carga-docente/{idDocente}")]
    public async Task<ActionResult<ReporteDocenteItem>> ObtenerReporteDocente(
        string idDocente,
        [FromQuery] string semestre,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(semestre))
            return BadRequest(new { mensaje = "El parámetro 'semestre' es obligatorio." });

        try
        {
            ReporteDocenteItem reporte =
                await _reporteCargaService.GenerarReportePorDocenteAsync(
                    idDocente, semestre, cancellationToken);

            return Ok(reporte);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Analiza todas las asignaciones del semestre y alerta sobre conflictos:
    /// cruces horarios, exceso de carga, asignaturas sin docente
    /// y docentes con asignaturas sin bloque horario definido.
    /// </summary>
    [HttpGet("conflictos")]
    public async Task<ActionResult<ConflictoAsignacionResponse>> ObtenerConflictos(
        [FromQuery] string semestre,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(semestre))
            return BadRequest(new { mensaje = "El parámetro 'semestre' es obligatorio." });

        ConflictoAsignacionResponse resultado =
            await _conflictoService.AnalizarConflictosAsync(semestre, cancellationToken);

        return Ok(resultado);
    }

    /// <summary>
    /// Genera y descarga el horario semanal en formato PDF para los escenarios indicados.
    /// </summary>
    [HttpGet("horario-pdf")]
    public async Task<IActionResult> ExportarHorarioPdf(
        [FromQuery] string semestre,
        [FromQuery] List<string>? escenarios,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(semestre))
            return BadRequest(new { mensaje = "El parámetro 'semestre' es obligatorio." });

        QuestPDF.Settings.License = LicenseType.Community;

        var escenariosTarget = (escenarios != null && escenarios.Count > 0)
            ? escenarios
            : new List<string> { "ING_DIURNA", "ING_NOCTURNA", "TAPSI_DIURNA", "TAPSI_NOCTURNA" };

        var asignaciones = await _context.Asignaciones
            .Include(a => a.Docente)
            .Include(a => a.Asignatura)
            .Where(a => a.Periodo == semestre && escenariosTarget.Contains(a.Escenario))
            .OrderBy(a => a.Escenario)
            .ThenBy(a => a.Dia)
            .ThenBy(a => a.HoraInicio)
            .ToListAsync(cancellationToken);

        string[] dias = { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado" };

        byte[] pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Text($"Horario Académico — Semestre {semestre}")
                    .FontSize(14).Bold().AlignCenter();

                page.Content().Column(col =>
                {
                    foreach (string escenario in escenariosTarget)
                    {
                        var bloques = asignaciones.Where(a => a.Escenario == escenario).ToList();
                        if (bloques.Count == 0) continue;

                        col.Item().PaddingTop(10).Text(escenario.Replace("_", " "))
                            .FontSize(11).Bold();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(50);
                                for (int i = 0; i < 6; i++) columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Hora").Bold();
                                foreach (string dia in dias)
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text(dia).Bold();
                            });

                            var horas = Enumerable.Range(7, 14).Select(h => $"{h:00}:00").ToList();
                            foreach (string hora in horas)
                            {
                                table.Cell().BorderBottom(0.5f).Padding(3).Text(hora);
                                for (int diaIdx = 1; diaIdx <= 6; diaIdx++)
                                {
                                    var bloque = bloques.FirstOrDefault(b =>
                                        b.Dia == diaIdx && b.HoraInicio == hora);

                                    if (bloque != null)
                                    {
                                        table.Cell().BorderBottom(0.5f).Background(Colors.Blue.Lighten4).Padding(3)
                                            .Text($"{bloque.Asignatura?.Nombre ?? ""}\n{bloque.Docente?.Nombre ?? ""}").FontSize(7);
                                    }
                                    else
                                    {
                                        table.Cell().BorderBottom(0.5f).Padding(3).Text("");
                                    }
                                }
                            }
                        });
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();

        return File(pdfBytes, "application/pdf", $"Horario_{semestre}.pdf");
    }
}