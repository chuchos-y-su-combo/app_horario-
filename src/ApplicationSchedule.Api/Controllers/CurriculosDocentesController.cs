using ApplicationSchedule.Application.DTOs.Curriculos;
using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Application.DTOs.Disponibilidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ApplicationSchedule.Api.Controllers;

/// <summary>
/// Controlador para cargar currículos docentes desde Excel
/// y consultar las asignaturas que cada docente puede dictar.
/// </summary>
[ApiController]
[Authorize]
[Route("api/profesores")]
public class CurriculosDocentesController : ControllerBase
{
    private readonly ICurriculoDocenteService _curriculoDocenteService;

    public CurriculosDocentesController(ICurriculoDocenteService curriculoDocenteService)
    {
        _curriculoDocenteService = curriculoDocenteService;
    }

    /// <summary>
    /// Importa desde Excel las asignaturas habilitadas y la disponibilidad de los docentes.
    /// </summary>
    [HttpPost("curriculos/importar-excel")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ImportarCurriculoResponse>> ImportarExcel(
        IFormFile archivo,
        CancellationToken cancellationToken)
    {
        if (archivo is null || archivo.Length == 0)
        {
            return BadRequest(new
            {
                mensaje = "Debe cargar un archivo Excel válido."
            });
        }

        string extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();

        if (extension != ".xlsx")
        {
            return BadRequest(new
            {
                mensaje = "El archivo debe tener formato .xlsx."
            });
        }

        await using Stream stream = archivo.OpenReadStream();

        ImportarCurriculoResponse resultado =
            await _curriculoDocenteService.ImportarDesdeExcelAsync(
                stream,
                archivo.FileName,
                cancellationToken
            );

        return Ok(resultado);
    }
    /// <summary>
    /// Obtiene las asignaturas que un docente está habilitado para dictar.
    /// </summary>
    [HttpGet("{idProfesor}/asignaturas-habilitadas")]
    public async Task<ActionResult<List<AsignaturaHabilitadaDocenteResponse>>> ObtenerAsignaturasHabilitadas(
        string idProfesor,
        CancellationToken cancellationToken)
    {
        try
        {
            List<AsignaturaHabilitadaDocenteResponse> asignaturas =
                await _curriculoDocenteService.ObtenerAsignaturasHabilitadasAsync(idProfesor, cancellationToken);

            return Ok(asignaturas);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                mensaje = ex.Message
            });
        }
    }

    /// <summary>
    /// Obtiene la disponibilidad importada para un docente.
    /// </summary>
    [HttpGet("{idProfesor}/disponibilidad")]
    public async Task<ActionResult<List<DisponibilidadDocenteResponse>>> ObtenerDisponibilidadDocente(
        string idProfesor,
        CancellationToken cancellationToken)
    {
        try
        {
            List<DisponibilidadDocenteResponse> disponibilidades =
                await _curriculoDocenteService.ObtenerDisponibilidadDocenteAsync(idProfesor, cancellationToken);

            return Ok(disponibilidades);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                mensaje = ex.Message
            });
        }
    }
    [HttpPost("{idDocente}/reducir-disponibilidad")]
    public async Task<ActionResult<ReduccionDisponibilidadResponse>> ReducirDisponibilidad(
    string idDocente,
    [FromQuery] string idAsignatura,
    [FromQuery] string periodo,
    CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idAsignatura))
            return BadRequest(new { mensaje = "El parámetro 'idAsignatura' es obligatorio." });

        if (string.IsNullOrWhiteSpace(periodo))
            return BadRequest(new { mensaje = "El parámetro 'periodo' es obligatorio." });

        try
        {
            ReduccionDisponibilidadResponse resultado =
                await _curriculoDocenteService.ReducirDisponibilidadPorDobleJornadaAsync(
                    idDocente, idAsignatura, periodo, cancellationToken);

            return Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }
}