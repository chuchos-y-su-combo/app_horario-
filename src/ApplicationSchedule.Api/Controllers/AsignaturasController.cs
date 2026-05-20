using ApplicationSchedule.Application.DTOs.Asignaturas;
using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Application.DTOs.Tapsi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApplicationSchedule.Application.Security;

namespace ApplicationSchedule.Api.Controllers;
[Authorize(Roles = RolesSistema.AdministradorOCoordinador)]
[ApiController]
[Route("api/asignaturas")]
/// <summary>
/// Controlador para la gestión de asignaturas.
/// Proporciona endpoints para consulta, creación, actualización y eliminación de asignaturas,
/// así como operaciones específicas relacionadas con reglas TAPSI.
/// </summary>
public class AsignaturasController : ControllerBase
{
    private readonly IAsignaturaService _asignaturaService;

    /// <summary>
    /// Constructor de <see cref="AsignaturasController"/>.
    /// </summary>
    /// <param name="asignaturaService">Servicio de aplicación para operaciones de asignaturas.</param>
    public AsignaturasController(IAsignaturaService asignaturaService)
    {
        _asignaturaService = asignaturaService;
    }

    /// <summary>
    /// Obtiene todas las asignaturas registradas.
    /// </summary>
    /// <returns>Lista de <see cref="AsignaturaResponse"/>.</returns>
    [HttpGet]
    public async Task<ActionResult<List<AsignaturaResponse>>> ObtenerTodas()
    {
        List<AsignaturaResponse> asignaturas = await _asignaturaService.ObtenerTodasAsync();

        return Ok(asignaturas);
    }

    /// <summary>
    /// Obtiene las asignaturas pertenecientes a un plan de estudio.
    /// </summary>
    /// <param name="idPlan">Identificador del plan de estudio.</param>
    /// <returns>Lista de asignaturas del plan.</returns>
    [HttpGet("plan/{idPlan}")]
    public async Task<ActionResult<List<AsignaturaResponse>>> ObtenerPorPlan(string idPlan)
    {
        List<AsignaturaResponse> asignaturas = await _asignaturaService.ObtenerPorPlanAsync(idPlan);

        return Ok(asignaturas);
    }

    /// <summary>
    /// Obtiene las asignaturas marcadas como fijas según las reglas TAPSI.
    /// </summary>
    /// <returns>Listado de asignaturas fijas TAPSI.</returns>
    [HttpGet("tapsi/fijas")]
    public async Task<ActionResult<List<AsignaturaResponse>>> ObtenerFijasTapsi()
    {
        List<AsignaturaResponse> asignaturas = await _asignaturaService.ObtenerFijasTapsiAsync();

        return Ok(asignaturas);
    }

    /// <summary>
    /// Obtiene las opciones adicionales TAPSI para la jornada diurna.
    /// </summary>
    /// <returns>Listado de asignaturas opcionales para TAPSI diurna.</returns>
    [HttpGet("tapsi/diurna/opciones-adicionales")]
    public async Task<ActionResult<List<AsignaturaResponse>>> ObtenerOpcionesAdicionalesTapsiDiurna()
    {
        List<AsignaturaResponse> asignaturas = await _asignaturaService.ObtenerOpcionalesTapsiDiurnaAsync();

        return Ok(asignaturas);
    }

    /// <summary>
    /// Obtiene el plan TAPSI para la jornada diurna.
    /// </summary>
    /// <returns>Información agregada del plan TAPSI diurno.</returns>
    [HttpGet("tapsi/diurna/plan")]
    public async Task<ActionResult<TapsiDiurnaPlanResponse>> ObtenerPlanTapsiDiurna()
    {
        TapsiDiurnaPlanResponse plan = await _asignaturaService.ObtenerPlanTapsiDiurnaAsync();

        return Ok(plan);
    }

    /// <summary>
    /// Marca las asignaturas adicionales TAPSI para jornada diurna como seleccionadas.
    /// </summary>
    /// <returns>Resultado con la cantidad marcada.</returns>
    [HttpPost("tapsi/diurna/marcar-opciones-adicionales")]
    public async Task<IActionResult> MarcarOpcionesAdicionalesTapsiDiurna()
    {
        int cantidadMarcada = await _asignaturaService.MarcarOpcionalesTapsiDiurnaAsync();

        return Ok(new
        {
            mensaje = "Asignaturas adicionales TAPSI para jornada diurna marcadas correctamente.",
            cantidadMarcada
        });
    }

    /// <summary>
    /// Marca las asignaturas obligatorias TAPSI como fijas.
    /// </summary>
    /// <returns>Resultado con la cantidad marcada.</returns>
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

    /// <summary>
    /// Obtiene una asignatura por su identificador.
    /// </summary>
    /// <param name="idAsignatura">Identificador de la asignatura.</param>
    /// <returns><see cref="AsignaturaResponse"/> si existe; 404 en caso contrario.</returns>
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

    /// <summary>
    /// Crea una nueva asignatura.
    /// </summary>
    /// <param name="request">Datos para crear la asignatura.</param>
    /// <returns>201 Created con la asignatura creada.</returns>
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

    /// <summary>
    /// Actualiza una asignatura existente.
    /// </summary>
    /// <param name="idAsignatura">Identificador de la asignatura.</param>
    /// <param name="request">Datos a actualizar.</param>
    /// <returns>204 No Content si se actualiza; 404 si no existe; 400 en caso de validación.</returns>
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

    /// <summary>
    /// Elimina una asignatura por su identificador.
    /// </summary>
    /// <param name="idAsignatura">Identificador de la asignatura a eliminar.</param>
    /// <returns>204 No Content si se eliminó; 404 si no existe.</returns>
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