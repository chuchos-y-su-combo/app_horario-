using ApplicationSchedule.Application.DTOs.Profesores;
using ApplicationSchedule.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationSchedule.Api.Controllers;

[ApiController]
[Route("api/profesores")]
public class ProfesoresController : ControllerBase
{
    private readonly IProfesorService _profesorService;

    public ProfesoresController(IProfesorService profesorService)
    {
        _profesorService = profesorService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProfesorResponse>>> ObtenerTodos()
    {
        List<ProfesorResponse> profesores = await _profesorService.ObtenerTodosAsync();

        return Ok(profesores);
    }

    [HttpGet("{idProfesor}")]
    public async Task<ActionResult<ProfesorResponse>> ObtenerPorId(string idProfesor)
    {
        ProfesorResponse? profesor = await _profesorService.ObtenerPorIdAsync(idProfesor);

        if (profesor is null)
        {
            return NotFound(new
            {
                mensaje = "Docente no encontrado."
            });
        }

        return Ok(profesor);
    }

    [HttpPost]
    public async Task<ActionResult<ProfesorResponse>> Crear(CrearProfesorRequest request)
    {
        try
        {
            ProfesorResponse profesorCreado = await _profesorService.CrearAsync(request);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { idProfesor = profesorCreado.IdProfesor },
                profesorCreado
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

    [HttpPut("{idProfesor}")]
    public async Task<IActionResult> Actualizar(string idProfesor, ActualizarProfesorRequest request)
    {
        try
        {
            bool actualizado = await _profesorService.ActualizarAsync(idProfesor, request);

            if (!actualizado)
            {
                return NotFound(new
                {
                    mensaje = "Docente no encontrado."
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

    [HttpDelete("{idProfesor}")]
    public async Task<IActionResult> Eliminar(string idProfesor)
    {
        try
        {
            bool eliminado = await _profesorService.EliminarAsync(idProfesor);

            if (!eliminado)
            {
                return NotFound(new
                {
                    mensaje = "Docente no encontrado."
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