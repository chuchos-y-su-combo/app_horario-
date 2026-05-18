using ApplicationSchedule.Application.DTOs.Horarios;
using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ApplicationSchedule.Infrastructure.Services;

public class CalendarioSemanalService : ICalendarioSemanalService
{
    private readonly AppDbContext _context;

    private static readonly string[] EstadosVisibles =
        { "Propuesta", "AsignadaManual", "Confirmada" };

    private static readonly Dictionary<int, string> NombresDias = new()
    {
        { 1, "Lunes" },
        { 2, "Martes" },
        { 3, "Miércoles" },
        { 4, "Jueves" },
        { 5, "Viernes" },
        { 6, "Sábado" }
    };

    public CalendarioSemanalService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CalendarioSemanalResponse> ObtenerCalendarioAsync(
        string semestre,
        string? idPlan = null,
        string? jornada = null,
        CancellationToken cancellationToken = default)
    {
        string semestreLimpio = semestre.Trim();
        string? idPlanLimpio = string.IsNullOrWhiteSpace(idPlan) ? null : idPlan.Trim();
        string? jornadaLimpia = string.IsNullOrWhiteSpace(jornada) ? null : jornada.Trim();

        // Traer todas las asignaciones activas del semestre con sus relaciones
        IQueryable<Asignacion> query = _context.Asignaciones
            .Include(a => a.Docente)
            .Include(a => a.Asignatura)
                .ThenInclude(asig => asig!.PlanEstudio)
            .Where(a =>
                a.Periodo == semestreLimpio &&
                EstadosVisibles.Contains(a.Estado) &&
                !string.IsNullOrEmpty(a.HoraInicio) &&
                !string.IsNullOrEmpty(a.HoraFin));

        // Filtro por plan
        if (idPlanLimpio is not null)
        {
            query = query.Where(a => a.Asignatura!.IdPlan == idPlanLimpio);
        }

        // Filtro por jornada (diurna/nocturna según el plan de estudios)
        if (jornadaLimpia is not null)
        {
            query = query.Where(a =>
                a.Asignatura!.PlanEstudio!.Jornada.ToLower() == jornadaLimpia.ToLower());
        }

        List<Asignacion> asignaciones = await query
            .OrderBy(a => a.Dia)
            .ThenBy(a => a.HoraInicio)
            .ToListAsync(cancellationToken);

        // Construir los días de la semana con sus bloques
        List<DiaSemanaCalendario> dias = NombresDias
            .Select(kv => new DiaSemanaCalendario
            {
                NumeroDia = kv.Key,
                NombreDia = kv.Value,
                Bloques = asignaciones
                    .Where(a => a.Dia == kv.Key)
                    .Select(a => new BloqueCalendario
                    {
                        IdAsignacion = a.IdAsignacion,
                        HoraInicio = a.HoraInicio,
                        HoraFin = a.HoraFin,
                        NombreAsignatura = a.Asignatura?.Nombre ?? string.Empty,
                        CodigoAsignatura = a.Asignatura?.Codigo ?? string.Empty,
                        NombreDocente = a.Docente?.Nombre ?? string.Empty,
                        Escenario = a.Escenario,
                        Jornada = a.Asignatura?.PlanEstudio?.Jornada ?? string.Empty,
                        NombrePlan = a.Asignatura?.PlanEstudio?.NombrePlan ?? string.Empty,
                        IdPlan = a.Asignatura?.IdPlan ?? string.Empty,
                        Estado = a.Estado
                    })
                    .OrderBy(b => b.HoraInicio)
                    .ToList()
            })
            .ToList();

        return new CalendarioSemanalResponse
        {
            Semestre = semestreLimpio,
            IdPlanFiltro = idPlanLimpio,
            JornadaFiltro = jornadaLimpia,
            Dias = dias
        };
    }
}