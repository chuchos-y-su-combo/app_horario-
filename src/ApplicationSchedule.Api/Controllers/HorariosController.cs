using ApplicationSchedule.Application.DTOs.Horarios;
using ApplicationSchedule.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationSchedule.Api.Controllers;

[ApiController]
[Route("api/horarios")]
public class HorariosController : ControllerBase
{
    private readonly IGeneradorHorarioService _generadorHorarioService;

    public HorariosController(IGeneradorHorarioService generadorHorarioService)
    {
        _generadorHorarioService = generadorHorarioService;
    }

    /// <summary>
    /// Genera automáticamente propuestas de asignación para los escenarios:
    /// Ingeniería diurna, Ingeniería nocturna, TAPSI diurna y TAPSI nocturna.
    /// </summary>
    [HttpPost("generar-propuestas")]
    public async Task<ActionResult<GenerarPropuestasHorarioResponse>> GenerarPropuestas(
        GenerarPropuestasHorarioRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            GenerarPropuestasHorarioResponse response =
                await _generadorHorarioService.GenerarPropuestasAsync(request, cancellationToken);

            return Ok(response);
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