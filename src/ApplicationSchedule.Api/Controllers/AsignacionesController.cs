using ApplicationSchedule.Application.DTOs.Asignaciones;
using ApplicationSchedule.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApplicationSchedule.Application.Security;

namespace ApplicationSchedule.Api.Controllers;
[Authorize(Roles = RolesSistema.AdministradorOCoordinador)] 
[ApiController]
[Route("api/asignaciones")]
public class AsignacionesController : ControllerBase
{
    private readonly IAsignacionService _asignacionService;

    public AsignacionesController(IAsignacionService asignacionService)
    {
        _asignacionService = asignacionService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AsignacionResponse>>> ObtenerTodas()
    {
        List<AsignacionResponse> asignaciones = await _asignacionService.ObtenerTodasAsync();
        return Ok(asignaciones);
    }

    [HttpGet("periodos-historicos")]
    public async Task<ActionResult<List<string>>> ObtenerPeriodosHistoricos()
    {
        List<string> periodos = await _asignacionService.ObtenerPeriodosHistoricosAsync();
        return Ok(periodos);
    }

    [HttpGet("consulta-historica")]
    public async Task<ActionResult<List<AsignacionResponse>>> ObtenerConsultaFiltrada(
        [FromQuery] int? semestre,
        [FromQuery] string? idDocente,
        [FromQuery] string? idAsignatura,
        [FromQuery] string? periodo)
    {
        List<AsignacionResponse> asignaciones = await _asignacionService.ObtenerFiltradasAsync(semestre, idDocente, idAsignatura, periodo);
        return Ok(asignaciones);
    }

    [HttpGet("docente/{idDocente}")]
    public async Task<ActionResult<List<AsignacionResponse>>> ObtenerPorDocente(
        string idDocente, [FromQuery] string? periodo = null)
    {
        try
        {
            List<AsignacionResponse> asignaciones = await _asignacionService.ObtenerPorDocenteAsync(idDocente, periodo);
            return Ok(asignaciones);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpGet("docente/{idDocente}/resumen")]
    public async Task<ActionResult<ResumenCargaDocenteResponse>> ObtenerResumenCargaDocente(
        string idDocente, [FromQuery] string periodo)
    {
        if (string.IsNullOrWhiteSpace(periodo))
            return BadRequest(new { mensaje = "El periodo es obligatorio." });

        ResumenCargaDocenteResponse? resumen =
            await _asignacionService.ObtenerResumenCargaDocenteAsync(idDocente, periodo);

        if (resumen is null)
            return NotFound(new { mensaje = "Docente no encontrado." });

        return Ok(resumen);
    }

    [HttpPost]
    public async Task<ActionResult<AsignacionResponse>> Crear(CrearAsignacionRequest request)
    {
        try
        {
            AsignacionResponse asignacionCreada = await _asignacionService.CrearAsync(request);
            return Created(string.Empty, asignacionCreada);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpDelete("{idAsignacion}")]
    public async Task<IActionResult> Eliminar(string idAsignacion)
    {
        bool eliminado = await _asignacionService.EliminarAsync(idAsignacion);

        if (!eliminado)
            return NotFound(new { mensaje = "Asignación no encontrada." });

        return NoContent();
    }

    [HttpPost("manual")]
    public async Task<ActionResult<AsignacionResponse>> AsignarManualmente(
        AsignarAsignaturaManualRequest request)
    {
        try
        {
            AsignacionResponse asignacion = await _asignacionService.AsignarManualmenteAsync(request);
            return Created(string.Empty, asignacion);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("docente/{idDocente}/asignaturas-disponibles")]
    public async Task<ActionResult<List<AsignaturaDisponibleParaDocenteResponse>>> ObtenerAsignaturasDisponibles(
        string idDocente, [FromQuery] string periodo)
    {
        if (string.IsNullOrWhiteSpace(periodo))
            return BadRequest(new { mensaje = "El parámetro 'periodo' es obligatorio." });

        try
        {
            var disponibles = await _asignacionService
                .ObtenerAsignaturasDisponiblesParaDocenteAsync(idDocente, periodo);

            return Ok(disponibles);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }
    [HttpGet("propuestas")]
    public async Task<ActionResult<List<AsignacionResponse>>> ObtenerPropuestas(
        [FromQuery] string periodo)
    {
        if (string.IsNullOrWhiteSpace(periodo))
            return BadRequest(new { mensaje = "El parámetro 'periodo' es obligatorio." });

        List<AsignacionResponse> propuestas =
            await _asignacionService.ObtenerPropuestasPorPeriodoAsync(periodo);

        return Ok(propuestas);
    }

    [HttpPatch("{idAsignacion}/ajustar")]
    public async Task<ActionResult<AsignacionResponse>> Ajustar(
        string idAsignacion,
        AjustarAsignacionRequest request)
    {
        try
        {
            AsignacionResponse actualizada =
                await _asignacionService.AjustarAsync(idAsignacion, request);

            return Ok(actualizada);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("confirmar")]
    public async Task<ActionResult<ResultadoConfirmacionResponse>> Confirmar(
        ConfirmarAsignacionesRequest request)
    {
        ResultadoConfirmacionResponse resultado =
            await _asignacionService.ConfirmarAsync(request);

        return Ok(resultado);
    }

    [HttpPatch("{idAsignacion}/cancelar")]
    public async Task<ActionResult<AsignacionResponse>> Cancelar(string idAsignacion)
    {
        try
        {
            AsignacionResponse cancelada =
                await _asignacionService.CancelarAsync(idAsignacion);

            return Ok(cancelada);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
    /// <summary>
    /// Coloca una asignación en un día de la semana.
    /// Opcionalmente define también el bloque horario.
    /// Una vez con día y hora definidos, aparece en el calendario semanal.
    /// Solo aplica a asignaciones en estado Propuesta o AsignadaManual.
    /// </summary>
    [HttpPatch("{idAsignacion}/asignar-dia")]
    public async Task<ActionResult<AsignacionResponse>> AsignarDia(
        string idAsignacion,
        AsignarDiaRequest request)
    {
        try
        {
            AsignacionResponse actualizada =
                await _asignacionService.AsignarDiaAsync(idAsignacion, request);

            return Ok(actualizada);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}