using ApplicationSchedule.Application.DTOs.Asignaciones;
using ApplicationSchedule.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationSchedule.Api.Controllers;

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

    [HttpGet("docente/{idDocente}")]
    public async Task<ActionResult<List<AsignacionResponse>>> ObtenerPorDocente(string idDocente, [FromQuery] string? periodo = null)
    {
        try
        {
            List<AsignacionResponse> asignaciones = await _asignacionService.ObtenerPorDocenteAsync(idDocente, periodo);

            return Ok(asignaciones);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                mensaje = ex.Message
            });
        }
    }

    [HttpGet("docente/{idDocente}/resumen")]
    public async Task<ActionResult<ResumenCargaDocenteResponse>> ObtenerResumenCargaDocente(string idDocente, [FromQuery] string periodo)
    {
        if (string.IsNullOrWhiteSpace(periodo))
        {
            return BadRequest(new
            {
                mensaje = "El periodo es obligatorio."
            });
        }

        ResumenCargaDocenteResponse? resumen = await _asignacionService.ObtenerResumenCargaDocenteAsync(idDocente, periodo);

        if (resumen is null)
        {
            return NotFound(new
            {
                mensaje = "Docente no encontrado."
            });
        }

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
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }

    [HttpDelete("{idAsignacion}")]
    public async Task<IActionResult> Eliminar(string idAsignacion)
    {
        bool eliminado = await _asignacionService.EliminarAsync(idAsignacion);

        if (!eliminado)
        {
            return NotFound(new
            {
                mensaje = "Asignación no encontrada."
            });
        }

        return NoContent();
    }
}