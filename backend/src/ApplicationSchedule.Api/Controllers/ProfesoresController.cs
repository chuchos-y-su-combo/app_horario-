using ApplicationSchedule.Application.DTOs.Disponibilidades;
using ApplicationSchedule.Application.DTOs.Profesores;
using ApplicationSchedule.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApplicationSchedule.Application.Security;

namespace ApplicationSchedule.Api.Controllers;
[Authorize(Roles = RolesSistema.AdministradorOCoordinador)]
[ApiController]
[Route("api/profesores")]
/// <summary>
/// Controlador para la gestión de docentes (profesores).
/// Expone operaciones CRUD y delega la lógica a <see cref="IProfesorService"/>.
/// </summary>
public class ProfesoresController : ControllerBase
{
    private readonly IProfesorService _profesorService;

    /// <summary>
    /// Constructor de <see cref="ProfesoresController"/>.
    /// </summary>
    /// <param name="profesorService">Servicio de dominio para operaciones sobre profesores.</param>
    public ProfesoresController(IProfesorService profesorService)
    {
        _profesorService = profesorService;
    }

    /// <summary>
    /// Recupera todos los docentes registrados en el sistema.
    /// </summary>
    /// <returns>Lista de <see cref="ProfesorResponse"/>.</returns>
    [HttpGet]
    public async Task<ActionResult<List<ProfesorResponse>>> ObtenerTodos()
    {
        List<ProfesorResponse> profesores = await _profesorService.ObtenerTodosAsync();

        return Ok(profesores);
    }

    /// <summary>
    /// Obtiene un docente por su identificador único.
    /// </summary>
    /// <param name="idProfesor">Identificador del docente.</param>
    /// <returns><see cref="ProfesorResponse"/> si existe; 404 en caso contrario.</returns>
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

    /// <summary>
    /// Crea un nuevo registro de docente.
    /// </summary>
    /// <param name="request">Datos para crear el docente.</param>
    /// <returns>201 Created con <see cref="ProfesorResponse"/> del docente creado.</returns>
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

    /// <summary>
    /// Actualiza un docente existente.
    /// </summary>
    /// <param name="idProfesor">Identificador del docente.</param>
    /// <param name="request">Datos con campos a actualizar.</param>
    /// <returns>204 No Content si se actualiza; 404 si no existe; 400 en caso de error.</returns>
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

    /// <summary>
    /// Retorna las franjas de disponibilidad horaria registradas para un docente.
    /// </summary>
    /// <param name="idProfesor">Identificador del docente.</param>
    /// <returns>Lista de <see cref="DisponibilidadDocenteResponse"/>.</returns>
    [HttpGet("{idProfesor}/disponibilidad")]
    public async Task<ActionResult<List<DisponibilidadDocenteResponse>>> ObtenerDisponibilidad(string idProfesor)
    {
        var disponibilidades = await _profesorService.ObtenerDisponibilidadAsync(idProfesor);
        return Ok(disponibilidades);
    }

    /// <summary>
    /// Elimina un docente por su identificador.
    /// </summary>
    /// <param name="idProfesor">Identificador del docente a eliminar.</param>
    /// <returns>204 No Content si se eliminó; 404 si no existe.</returns>
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