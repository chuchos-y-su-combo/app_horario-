using ApplicationSchedule.Application.DTOs.Horarios;
using ApplicationSchedule.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ApplicationSchedule.Api.Controllers;
[Authorize]
[ApiController]
[Route("api/horarios")]
public class HorariosController : ControllerBase
{
    private readonly IGeneradorHorarioService _generadorHorarioService;
    private readonly IHorarioExportService _horarioExportService;
    private readonly ICalendarioSemanalService _calendarioSemanalService;

    public HorariosController(
        IGeneradorHorarioService generadorHorarioService,
        IHorarioExportService horarioExportService,
        ICalendarioSemanalService calendarioSemanalService)
    {
        _generadorHorarioService = generadorHorarioService;
        _horarioExportService = horarioExportService;
        _calendarioSemanalService = calendarioSemanalService;
    }

    /// <summary>
    /// Genera automáticamente propuestas de asignación para los escenarios:
    /// Ingeniería diurna, Ingeniería nocturna, TAPSI diurna y TAPSI nocturna.
    /// </summary>
    [HttpPost("generar-propuestas")]
    public async Task<ActionResult<GenerarPropuestasHorarioResponse>> GenerarPropuestas(
        GenerarPropuestasHorarioRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            GenerarPropuestasHorarioResponse response =
                await _generadorHorarioService.GenerarPropuestasAsync(request, cancellationToken);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Exporta el horario filtrado por semestre, docente o asignatura a Excel.
    /// </summary>
    [HttpGet("exportar")]
    public async Task<IActionResult> ExportarHorario(
        [FromQuery] int? semestre,
        [FromQuery] string? idDocente,
        [FromQuery] string? idAsignatura,
        [FromQuery] string? periodo)
    {
        var excelBytes = await _horarioExportService.ExportarHorariosAsync(
            semestre, idDocente, idAsignatura, periodo);

        var nombreArchivo = $"Horarios_Confirmados_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

        return File(
            excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            nombreArchivo);
    }

    /// <summary>
    /// Issue #39: Devuelve el horario en vista de calendario semanal (Lunes-Sábado),
    /// filtrable por plan de estudios y jornada.
    /// Solo incluye asignaciones con bloque horario definido.
    /// </summary>
    [HttpGet("calendario")]
    public async Task<ActionResult<CalendarioSemanalResponse>> ObtenerCalendario(
        [FromQuery] string semestre,
        [FromQuery] string? idPlan = null,
        [FromQuery] string? jornada = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(semestre))
            return BadRequest(new { mensaje = "El parámetro 'semestre' es obligatorio." });

        if (jornada is not null &&
            !jornada.Equals("Diurna", StringComparison.OrdinalIgnoreCase) &&
            !jornada.Equals("Nocturna", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                mensaje = "El parámetro 'jornada' debe ser 'Diurna' o 'Nocturna'."
            });
        }

        CalendarioSemanalResponse calendario =
            await _calendarioSemanalService.ObtenerCalendarioAsync(
                semestre, idPlan, jornada, cancellationToken);

        return Ok(calendario);
    }
    /// <summary>
    /// Issue #40: Devuelve el horario individual de un docente en vista de
    /// calendario semanal (Lunes-Sábado) para un semestre.
    /// Incluye datos del docente, total de asignaturas y horas semanales.
    /// </summary>
    [HttpGet("calendario/docente/{idDocente}")]
    public async Task<ActionResult<CalendarioDocenteResponse>> ObtenerCalendarioDocente(
        string idDocente,
        [FromQuery] string semestre,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(semestre))
            return BadRequest(new { mensaje = "El parámetro 'semestre' es obligatorio." });

        try
        {
            CalendarioDocenteResponse calendario =
                await _calendarioSemanalService.ObtenerCalendarioDocenteAsync(
                    idDocente, semestre, cancellationToken);

            return Ok(calendario);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }
}