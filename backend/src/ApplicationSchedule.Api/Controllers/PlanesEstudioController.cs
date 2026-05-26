using ApplicationSchedule.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ApplicationSchedule.Application.Security;

namespace ApplicationSchedule.Api.Controllers;

/// <summary>
/// Controlador para consultar los planes de estudio registrados en el sistema.
/// Los planes identifican cada combinación de carrera y jornada (Diurna/Nocturna)
/// y agrupan las asignaturas que les corresponden.
/// </summary>
[ApiController]
[Authorize(Roles = RolesSistema.AdministradorOCoordinador)]
[Route("api/planes-estudio")]
public class PlanesEstudioController : ControllerBase
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Inicializa el controlador con acceso directo al contexto de EF Core.
    /// Se usa el contexto directamente por ser un endpoint de solo lectura sin lógica de negocio.
    /// </summary>
    public PlanesEstudioController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Devuelve todos los planes de estudio ordenados por nombre.
    /// Incluye idPlan, nombrePlan y jornada para poblar selectores en la UI.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(CancellationToken cancellationToken)
    {
        var planes = await _context.PlanesEstudio
            .AsNoTracking()
            .OrderBy(p => p.NombrePlan)
            .Select(p => new
            {
                idPlan = p.IdPlan,
                nombrePlan = p.NombrePlan,
                jornada = p.Jornada
            })
            .ToListAsync(cancellationToken);

        return Ok(planes);
    }
}
