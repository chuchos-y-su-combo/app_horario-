using ApplicationSchedule.Application.DTOs.Horarios;
using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ApplicationSchedule.Infrastructure.Services;

/// <summary>
/// Servicio que construye vistas de calendario semanal y calendarios individuales de docentes.
/// Filtra y transforma asignaciones a estructuras aptas para presentación en UI o exportación.
/// </summary>
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

    /// <summary>
    /// Crea una instancia de <see cref="CalendarioSemanalService"/> con el contexto de datos.
    /// </summary>
    public CalendarioSemanalService(AppDbContext context)
    {
        _context = context;
    }

    // ── Issue #39 ──────────────────────────────────────────────────────────

    /// <summary>
    /// Obtiene el calendario semanal para un semestre, opcionalmente filtrado por plan y jornada.
    /// </summary>
    public async Task<CalendarioSemanalResponse> ObtenerCalendarioAsync(
        string semestre,
        string? idPlan = null,
        string? jornada = null,
        CancellationToken cancellationToken = default)
    {
        string semestreLimpio = semestre.Trim();
        string? idPlanLimpio = string.IsNullOrWhiteSpace(idPlan) ? null : idPlan.Trim();
        string? jornadaLimpia = string.IsNullOrWhiteSpace(jornada) ? null : jornada.Trim();

        IQueryable<Asignacion> query = QueryBaseConHorario(semestreLimpio);

        if (idPlanLimpio is not null)
            query = query.Where(a => a.Asignatura!.IdPlan == idPlanLimpio);

        if (jornadaLimpia is not null)
            query = query.Where(a =>
                a.Asignatura!.PlanEstudio!.Jornada.ToLower() == jornadaLimpia.ToLower());

        List<Asignacion> asignaciones = await query
            .OrderBy(a => a.Dia)
            .ThenBy(a => a.HoraInicio)
            .ToListAsync(cancellationToken);

        return new CalendarioSemanalResponse
        {
            Semestre = semestreLimpio,
            IdPlanFiltro = idPlanLimpio,
            JornadaFiltro = jornadaLimpia,
            Dias = ConstruirDias(asignaciones)
        };
    }

    // ── Issue #40 ──────────────────────────────────────────────────────────

    /// <summary>
    /// Obtiene el calendario semanal individual de un docente y resumen de su carga semanal.
    /// </summary>
    public async Task<CalendarioDocenteResponse> ObtenerCalendarioDocenteAsync(
        string idDocente,
        string semestre,
        CancellationToken cancellationToken = default)
    {
        string semestreLimpio = semestre.Trim();

        Docente? docente = await _context.Docentes
            .FirstOrDefaultAsync(d => d.IdDocente == idDocente, cancellationToken);

        if (docente is null)
            throw new InvalidOperationException("Docente no encontrado.");

        // Todas las asignaciones del docente (con y sin horario)
        // para contar asignaturas y horas correctamente
        List<Asignacion> todasLasAsignaciones = await _context.Asignaciones
            .Include(a => a.Asignatura)
                .ThenInclude(asig => asig!.PlanEstudio)
            .Where(a =>
                a.IdDocente == idDocente &&
                a.Periodo == semestreLimpio &&
                EstadosVisibles.Contains(a.Estado))
            .ToListAsync(cancellationToken);

        // Solo las que tienen horario van al calendario
        List<Asignacion> conHorario = todasLasAsignaciones
            .Where(a =>
                !string.IsNullOrEmpty(a.HoraInicio) &&
                !string.IsNullOrEmpty(a.HoraFin))
            .OrderBy(a => a.Dia)
            .ThenBy(a => a.HoraInicio)
            .ToList();

        int totalAsignaturas = todasLasAsignaciones
            .Select(a => a.IdAsignatura)
            .Distinct()
            .Count();

        double totalHoras = conHorario
            .Sum(a => CalcularHoras(a.HoraInicio, a.HoraFin));

        return new CalendarioDocenteResponse
        {
            IdDocente = docente.IdDocente,
            NombreDocente = docente.Nombre,
            Identificacion = docente.Identificacion,
            TipoContrato = docente.TipoContrato,
            MaxAsignaturas = docente.MaxAsignaturas,
            Semestre = semestreLimpio,
            TotalAsignaturas = totalAsignaturas,
            TotalHorasSemanales = Math.Round(totalHoras, 2),
            Dias = ConstruirDias(conHorario)
        };
    }

    // ── Privados ───────────────────────────────────────────────────────────

    private IQueryable<Asignacion> QueryBaseConHorario(string semestre)
    {
        return _context.Asignaciones
            .Include(a => a.Docente)
            .Include(a => a.Asignatura)
                .ThenInclude(asig => asig!.PlanEstudio)
            .Where(a =>
                a.Periodo == semestre &&
                EstadosVisibles.Contains(a.Estado) &&
                !string.IsNullOrEmpty(a.HoraInicio) &&
                !string.IsNullOrEmpty(a.HoraFin));
    }

    private static List<DiaSemanaCalendario> ConstruirDias(List<Asignacion> asignaciones)
    {
        return NombresDias
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
    }

    private static double CalcularHoras(string horaInicio, string horaFin)
    {
        if (!TimeSpan.TryParse(horaInicio, out TimeSpan inicio) ||
            !TimeSpan.TryParse(horaFin, out TimeSpan fin))
            return 0;

        double horas = (fin - inicio).TotalHours;
        return horas > 0 ? horas : 0;
    }
}