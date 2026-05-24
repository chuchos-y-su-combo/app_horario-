using ApplicationSchedule.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ApplicationSchedule.Application.Security;

namespace ApplicationSchedule.Api.Controllers;

[ApiController]
[Authorize(Roles = RolesSistema.AdministradorOCoordinador)]
[Route("api/planes-estudio")]
public class PlanesEstudioController : ControllerBase
{
    private readonly AppDbContext _context;

    public PlanesEstudioController(AppDbContext context)
    {
        _context = context;
    }

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
