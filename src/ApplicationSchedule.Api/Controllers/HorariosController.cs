using ApplicationSchedule.Application.DTOs.Horarios;
using ApplicationSchedule.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationSchedule.Api.Controllers;

[ApiController]
[Route("api/horarios")]
public class HorariosController : ControllerBase
{
    private readonly IGeneradorHorarioService _generadorHorarioService;
    private readonly IHorarioExportService _horarioExportService;

    public HorariosController(
        IGeneradorHorarioService generadorHorarioService,
        IHorarioExportService horarioExportService)
    {
        _generadorHorarioService = generadorHorarioService;
        _horarioExportService = horarioExportService;
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
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }

    /// <summary>
    /// RF-22, RF-23: Exporta el horario filtrado por semestre, docente o asignatura a un archivo Excel (.xlsx).
    /// </summary>
    [HttpGet("exportar")]
    public async Task<IActionResult> ExportarHorario(
        [FromQuery] int? semestre,
        [FromQuery] string? idDocente,
        [FromQuery] string? idAsignatura,
        [FromQuery] string? periodo)
    {
        var excelBytes = await _horarioExportService.ExportarHorariosAsync(semestre, idDocente, idAsignatura, periodo);
        var nombreArchivo = $"Horarios_Confirmados_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
    }
}