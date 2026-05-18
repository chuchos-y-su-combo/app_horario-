using ApplicationSchedule.Application.DTOs.Bloqueos;
using ApplicationSchedule.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApplicationSchedule.Application.Security;

namespace ApplicationSchedule.Api.Controllers;

[Authorize(Roles = RolesSistema.AdministradorOCoordinador)]
[ApiController]
[Route("api/asignaturas")]
public class BloqueosFranjaAsignaturaController : ControllerBase
{
    private readonly IBloqueoFranjaAsignaturaService _bloqueoService;

    public BloqueosFranjaAsignaturaController(IBloqueoFranjaAsignaturaService bloqueoService)
    {
        _bloqueoService = bloqueoService;
    }

    /// <summary>
    /// Lista todas las franjas bloqueadas. Puede filtrarse por periodo.
    /// </summary>
    [HttpGet("bloqueos-franja")]
    public async Task<ActionResult<List<BloqueoFranjaAsignaturaResponse>>> ObtenerTodos(
        [FromQuery] string? periodo,
        CancellationToken cancellationToken)
    {
        List<BloqueoFranjaAsignaturaResponse> bloqueos =
            await _bloqueoService.ObtenerTodosAsync(periodo, cancellationToken);

        return Ok(bloqueos);
    }

    /// <summary>
    /// Lista las franjas bloqueadas de una asignatura específica.
    /// </summary>
    [HttpGet("{idAsignatura}/bloqueos-franja")]
    public async Task<ActionResult<List<BloqueoFranjaAsignaturaResponse>>> ObtenerPorAsignatura(
        string idAsignatura,
        [FromQuery] string? periodo,
        CancellationToken cancellationToken)
    {
        try
        {
            List<BloqueoFranjaAsignaturaResponse> bloqueos =
                await _bloqueoService.ObtenerPorAsignaturaAsync(
                    idAsignatura,
                    periodo,
                    cancellationToken
                );

            return Ok(bloqueos);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Bloquea una franja horaria para una asignatura específica.
    /// El generador automático y los ajustes manuales no podrán ubicar esa asignatura en esa franja.
    /// </summary>
    [HttpPost("{idAsignatura}/bloqueos-franja")]
    public async Task<ActionResult<BloqueoFranjaAsignaturaResponse>> Crear(
        string idAsignatura,
        CrearBloqueoFranjaAsignaturaRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            BloqueoFranjaAsignaturaResponse bloqueo =
                await _bloqueoService.CrearAsync(idAsignatura, request, cancellationToken);

            return Created(string.Empty, bloqueo);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Elimina una franja bloqueada.
    /// </summary>
    [HttpDelete("bloqueos-franja/{idBloqueo}")]
    public async Task<IActionResult> Eliminar(
        string idBloqueo,
        CancellationToken cancellationToken)
    {
        bool eliminado = await _bloqueoService.EliminarAsync(idBloqueo, cancellationToken);

        if (!eliminado)
            return NotFound(new { mensaje = "Bloqueo de franja no encontrado." });

        return NoContent();
    }
}