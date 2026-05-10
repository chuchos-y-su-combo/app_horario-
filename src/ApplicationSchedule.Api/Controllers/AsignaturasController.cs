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

    [HttpGet("plan/{idPlan}")]
    public async Task<ActionResult<List<AsignaturaResponse>>> ObtenerPorPlan(string idPlan)
    {
        List<AsignaturaResponse> asignaturas = await _asignaturaService.ObtenerPorPlanAsync(idPlan);

        return Ok(asignaturas);
    }

    [HttpGet("tapsi/fijas")]
    public async Task<ActionResult<List<AsignaturaResponse>>> ObtenerFijasTapsi()
    {
        List<AsignaturaResponse> asignaturas = await _asignaturaService.ObtenerFijasTapsiAsync();

        return Ok(asignaturas);
    }

    [HttpPost("tapsi/marcar-fijas")]
    public async Task<IActionResult> MarcarObligatoriasTapsiComoFijas()
    {
        int cantidadMarcada = await _asignaturaService.MarcarObligatoriasTapsiComoFijasAsync();

        return Ok(new
        {
            mensaje = "Asignaturas obligatorias TAPSI marcadas como fijas correctamente.",
            cantidadMarcada
        });
    }

    [HttpGet("{idAsignatura}")]
    public async Task<ActionResult<AsignaturaResponse>> ObtenerPorId(string idAsignatura)
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
    public async Task<IActionResult> Actualizar(string idAsignatura, ActualizarAsignaturaRequest request)
    {
        try
        {
            bool actualizada = await _asignaturaService.ActualizarAsync(idAsignatura, request);

            if (!actualizada)
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
    public async Task<IActionResult> Eliminar(string idAsignatura)
    {
        try
        {
            bool eliminada = await _asignaturaService.EliminarAsync(idAsignatura);

            if (!eliminada)
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
}