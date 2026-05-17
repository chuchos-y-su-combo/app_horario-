using ApplicationSchedule.Application.DTOs.Asignaciones;
using ApplicationSchedule.Application.DTOs.Horarios;
using ApplicationSchedule.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationSchedule.Api.Controllers;

[ApiController]
[Route("api/reportes")]
public class ReportesController : ControllerBase
{
    private readonly IReporteCargaService _reporteCargaService;
    private readonly IConflictoAsignacionService _conflictoService;

    public ReportesController(
        IReporteCargaService reporteCargaService,
        IConflictoAsignacionService conflictoService)
    {
        _reporteCargaService = reporteCargaService;
        _conflictoService = conflictoService;
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
}