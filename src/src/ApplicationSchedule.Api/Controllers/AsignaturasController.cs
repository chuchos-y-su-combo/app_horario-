using ApplicationSchedule.Application.DTOs.Asignaturas;
using ApplicationSchedule.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationSchedule.Api.Controllers;

[ApiController]
[Route("api/asignaturas")]
public class AsignaturasController : ControllerBase
{
    private readonly IAsignaturaService _asignaturaService;

    public AsignaturasController(IAsignaturaService asignaturaService)
    {
        _asignaturaService = asignaturaService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AsignaturaResponse>>> ObtenerTodas()
    {
        List<AsignaturaResponse> asignaturas = await _asignaturaService.ObtenerTodasAsync();

        return Ok(asignaturas);
    }

    [HttpGet("plan/{idPlanEstudios}")]
    public async Task<ActionResult<List<AsignaturaResponse>>> ObtenerPorPlanEstudios(int idPlanEstudios)
    {
        List<AsignaturaResponse> asignaturas = await _asignaturaService.ObtenerPorPlanEstudiosAsync(idPlanEstudios);

        return Ok(asignaturas);
    }

    [HttpGet("{idAsignatura}")]
    public async Task<ActionResult<AsignaturaResponse>> ObtenerPorId(int idAsignatura)
    {
        AsignaturaResponse? asignatura = await _asignaturaService.ObtenerPorIdAsync(idAsignatura);

        if (asignatura is null)
        {
            return NotFound(new
            {
                mensaje = "Asignatura no encontrada."
            });
        }

        return Ok(asignatura);
    }

    [HttpPost]
    public async Task<ActionResult<AsignaturaResponse>> Crear(CrearAsignaturaRequest request)
    {
        try
        {
            AsignaturaResponse asignaturaCreada = await _asignaturaService.CrearAsync(request);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { idAsignatura = asignaturaCreada.IdAsignatura },
                asignaturaCreada
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }

    [HttpPut("{idAsignatura}")]
    public async Task<IActionResult> Actualizar(int idAsignatura, ActualizarAsignaturaRequest request)
    {
        try
        {
            bool actualizado = await _asignaturaService.ActualizarAsync(idAsignatura, request);

            if (!actualizado)
            {
                return NotFound(new
                {
                    mensaje = "Asignatura no encontrada."
                });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }

    [HttpDelete("{idAsignatura}")]
    public async Task<IActionResult> Eliminar(int idAsignatura)
    {
        bool eliminado = await _asignaturaService.EliminarAsync(idAsignatura);

        if (!eliminado)
        {
            return NotFound(new
            {
                mensaje = "Asignatura no encontrada."
            });
        }

        return NoContent();
    }
}
