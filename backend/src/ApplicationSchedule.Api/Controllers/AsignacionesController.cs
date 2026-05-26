using ApplicationSchedule.Application.DTOs.Asignaciones;
using ApplicationSchedule.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApplicationSchedule.Application.Security;

namespace ApplicationSchedule.Api.Controllers;

/// <summary>
/// Controlador REST para gestión de asignaciones de horario.
/// Una asignación vincula un docente con una asignatura en un día y franja horaria específicos.
/// Los estados posibles son: Propuesta (generada automáticamente), AsignadaManual, Confirmada y Cancelada.
/// </summary>
[Authorize(Roles = RolesSistema.AdministradorOCoordinador)]
[ApiController]
[Route("api/asignaciones")]
public class AsignacionesController : ControllerBase
{
    private readonly IAsignacionService _asignacionService;

    /// <summary>
    /// Inicializa el controlador inyectando el servicio de asignaciones.
    /// </summary>
    public AsignacionesController(IAsignacionService asignacionService)
    {
        _asignacionService = asignacionService;
    }

    /// <summary>
    /// Devuelve todas las asignaciones registradas, sin filtrar por estado ni periodo.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<AsignacionResponse>>> ObtenerTodas()
    {
        List<AsignacionResponse> asignaciones = await _asignacionService.ObtenerTodasAsync();
        return Ok(asignaciones);
    }

    /// <summary>
    /// Lista los periodos académicos que tienen al menos una asignación registrada.
    /// Se usa para poblar selectores de periodos históricos en la UI.
    /// </summary>
    [HttpGet("periodos-historicos")]
    public async Task<ActionResult<List<string>>> ObtenerPeriodosHistoricos()
    {
        List<string> periodos = await _asignacionService.ObtenerPeriodosHistoricosAsync();
        return Ok(periodos);
    }

    /// <summary>
    /// Consulta histórica: devuelve asignaciones de cualquier estado (Propuesta y Confirmada)
    /// para el periodo indicado. Soporta filtros opcionales por semestre, docente y asignatura.
    /// </summary>
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

    /// <summary>
    /// Exportación JSON: igual a la consulta histórica pero filtra exclusivamente
    /// por Estado == "Confirmada", equivalente a lo que exporta el Excel.
    /// </summary>
    [HttpGet("exportar-json")]
    public async Task<ActionResult<List<AsignacionResponse>>> ObtenerExportacionJson(
        [FromQuery] int? semestre,
        [FromQuery] string? idDocente,
        [FromQuery] string? idAsignatura,
        [FromQuery] string? periodo)
    {
        List<AsignacionResponse> asignaciones = await _asignacionService.ObtenerFiltradasAsync(semestre, idDocente, idAsignatura, periodo, "Confirmada");
        return Ok(asignaciones);
    }

    /// <summary>
    /// Devuelve todas las asignaciones de un docente específico.
    /// Si se indica <paramref name="periodo"/>, filtra por ese periodo académico.
    /// </summary>
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

    /// <summary>
    /// Devuelve un resumen de la carga semanal de un docente en el periodo indicado:
    /// número de asignaturas distintas, total de horas semanales y detalle por asignatura.
    /// </summary>
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

    /// <summary>
    /// Crea una nueva asignación manual validando que no existan cruces de horario.
    /// </summary>
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

    /// <summary>
    /// Elimina permanentemente una asignación por su identificador.
    /// </summary>
    [HttpDelete("{idAsignacion}")]
    public async Task<IActionResult> Eliminar(string idAsignacion)
    {
        bool eliminado = await _asignacionService.EliminarAsync(idAsignacion);

        if (!eliminado)
            return NotFound(new { mensaje = "Asignación no encontrada." });

        return NoContent();
    }

    /// <summary>
    /// Asigna una asignatura a un docente de forma manual, respetando disponibilidad y cruces.
    /// El resultado queda en estado "AsignadaManual" hasta ser confirmado.
    /// </summary>
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

    /// <summary>
    /// Lista las asignaturas que un docente puede dictar y que aún no tienen asignación
    /// en el periodo indicado. Útil para el flujo de ajuste manual.
    /// </summary>
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

    /// <summary>
    /// Lista todas las asignaciones en estado "Propuesta" para el periodo indicado.
    /// Se usa en la vista de Generación para revisar propuestas antes de confirmarlas.
    /// </summary>
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

    /// <summary>
    /// Modifica el docente, la franja horaria u otros atributos de una asignación existente.
    /// Solo aplica a asignaciones en estado Propuesta o AsignadaManual.
    /// </summary>
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

    /// <summary>
    /// Confirma un lote de propuestas de asignación, cambiando su estado a "Confirmada".
    /// Las asignaciones confirmadas aparecen en reportes y exportaciones Excel.
    /// </summary>
    [HttpPost("confirmar")]
    public async Task<ActionResult<ResultadoConfirmacionResponse>> Confirmar(
        ConfirmarAsignacionesRequest request)
    {
        ResultadoConfirmacionResponse resultado =
            await _asignacionService.ConfirmarAsync(request);

        return Ok(resultado);
    }

    /// <summary>
    /// Cancela una asignación cambiando su estado a "Cancelada".
    /// La asignación permanece en la base de datos para auditoría.
    /// </summary>
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