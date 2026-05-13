using ApplicationSchedule.Application.DTOs.Curriculos;
using ApplicationSchedule.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationSchedule.Api.Controllers;

/// <summary>
/// Controlador para cargar currículos docentes desde Excel
/// y consultar las asignaturas que cada docente puede dictar.
/// </summary>
[ApiController]
[Route("api/profesores")]
public class CurriculosDocentesController : ControllerBase
{
    private readonly ICurriculoDocenteService _curriculoDocenteService;

    public CurriculosDocentesController(ICurriculoDocenteService curriculoDocenteService)
    {
        _curriculoDocenteService = curriculoDocenteService;
    }

    /// <summary>
    /// Importa un archivo Excel con hojas por docente y determina automáticamente
    /// las asignaturas que puede dictar cada uno.
    /// </summary>
    [HttpPost("curriculos/importar-excel")]
    public async Task<ActionResult<ImportarCurriculoResponse>> ImportarExcel(
        [FromForm] IFormFile archivo,
        CancellationToken cancellationToken)
    {
        if (archivo is null || archivo.Length == 0)
        {
            return BadRequest(new
            {
                mensaje = "Debe enviar un archivo Excel válido."
            });
        }

        string extension = Path.GetExtension(archivo.FileName);

        if (!string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                mensaje = "El archivo debe tener extensión .xlsx."
            });
        }

        try
        {
            await using Stream stream = archivo.OpenReadStream();

            ImportarCurriculoResponse resultado = await _curriculoDocenteService.ImportarDesdeExcelAsync(
                stream,
                archivo.FileName,
                cancellationToken
            );

            return Ok(resultado);
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
}