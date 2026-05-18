using ApplicationSchedule.Application.DTOs.Bloqueos;
using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ApplicationSchedule.Infrastructure.Services;

public class BloqueoFranjaAsignaturaService : IBloqueoFranjaAsignaturaService
{
    private readonly AppDbContext _context;

    public BloqueoFranjaAsignaturaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<BloqueoFranjaAsignaturaResponse>> ObtenerTodosAsync(
        string? periodo = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<BloqueoFranjaAsignatura> query = _context.BloqueosFranjaAsignatura
            .Include(b => b.Asignatura);

        if (!string.IsNullOrWhiteSpace(periodo))
        {
            string periodoLimpio = periodo.Trim();
            query = query.Where(b => b.Periodo == periodoLimpio);
        }

        List<BloqueoFranjaAsignatura> bloqueos = await query
            .OrderByDescending(b => b.Periodo)
            .ThenBy(b => b.Asignatura!.Nombre)
            .ThenBy(b => b.Dia)
            .ThenBy(b => b.HoraInicio)
            .ToListAsync(cancellationToken);

        return bloqueos.Select(ToResponse).ToList();
    }

    public async Task<List<BloqueoFranjaAsignaturaResponse>> ObtenerPorAsignaturaAsync(
        string idAsignatura,
        string? periodo = null,
        CancellationToken cancellationToken = default)
    {
        bool asignaturaExiste = await _context.Asignaturas
            .AnyAsync(a => a.IdAsignatura == idAsignatura, cancellationToken);

        if (!asignaturaExiste)
            throw new InvalidOperationException("Asignatura no encontrada.");

        IQueryable<BloqueoFranjaAsignatura> query = _context.BloqueosFranjaAsignatura
            .Include(b => b.Asignatura)
            .Where(b => b.IdAsignatura == idAsignatura);

        if (!string.IsNullOrWhiteSpace(periodo))
        {
            string periodoLimpio = periodo.Trim();
            query = query.Where(b => b.Periodo == periodoLimpio);
        }

        List<BloqueoFranjaAsignatura> bloqueos = await query
            .OrderByDescending(b => b.Periodo)
            .ThenBy(b => b.Dia)
            .ThenBy(b => b.HoraInicio)
            .ToListAsync(cancellationToken);

        return bloqueos.Select(ToResponse).ToList();
    }

    public async Task<BloqueoFranjaAsignaturaResponse> CrearAsync(
        string idAsignatura,
        CrearBloqueoFranjaAsignaturaRequest request,
        CancellationToken cancellationToken = default)
    {
        string periodo = request.Periodo.Trim();
        string horaInicio = request.HoraInicio.Trim();
        string horaFin = request.HoraFin.Trim();
        string? motivo = string.IsNullOrWhiteSpace(request.Motivo)
            ? null
            : request.Motivo.Trim();

        ValidarDatos(periodo, request.Dia, horaInicio, horaFin);

        Asignatura? asignatura = await _context.Asignaturas
            .FirstOrDefaultAsync(a => a.IdAsignatura == idAsignatura, cancellationToken);

        if (asignatura is null)
            throw new InvalidOperationException("Asignatura no encontrada.");

        List<BloqueoFranjaAsignatura> bloqueosExistentes = await _context.BloqueosFranjaAsignatura
            .Where(b =>
                b.IdAsignatura == idAsignatura &&
                b.Periodo == periodo &&
                b.Dia == request.Dia)
            .ToListAsync(cancellationToken);

        bool existeSolapamiento = bloqueosExistentes.Any(b =>
            HorariosSeCruzan(horaInicio, horaFin, b.HoraInicio, b.HoraFin));

        if (existeSolapamiento)
            throw new InvalidOperationException(
                "La asignatura ya tiene una franja bloqueada que se cruza con el horario enviado.");

        var bloqueo = new BloqueoFranjaAsignatura
        {
            IdBloqueo = Guid.NewGuid().ToString(),
            IdAsignatura = asignatura.IdAsignatura,
            Periodo = periodo,
            Dia = request.Dia,
            HoraInicio = horaInicio,
            HoraFin = horaFin,
            Motivo = motivo,
            FechaCreacionUtc = DateTime.UtcNow
        };

        _context.BloqueosFranjaAsignatura.Add(bloqueo);
        await _context.SaveChangesAsync(cancellationToken);

        bloqueo.Asignatura = asignatura;

        return ToResponse(bloqueo);
    }

    public async Task<bool> EliminarAsync(
        string idBloqueo,
        CancellationToken cancellationToken = default)
    {
        BloqueoFranjaAsignatura? bloqueo = await _context.BloqueosFranjaAsignatura
            .FirstOrDefaultAsync(b => b.IdBloqueo == idBloqueo, cancellationToken);

        if (bloqueo is null)
            return false;

        _context.BloqueosFranjaAsignatura.Remove(bloqueo);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static void ValidarDatos(string periodo, int dia, string horaInicio, string horaFin)
    {
        if (string.IsNullOrWhiteSpace(periodo))
            throw new InvalidOperationException("El periodo es obligatorio.");

        if (dia < 1 || dia > 6)
            throw new InvalidOperationException("El día debe estar entre 1 y 6.");

        if (!TimeSpan.TryParse(horaInicio, out TimeSpan inicio))
            throw new InvalidOperationException("La hora de inicio debe tener formato HH:mm.");

        if (!TimeSpan.TryParse(horaFin, out TimeSpan fin))
            throw new InvalidOperationException("La hora de fin debe tener formato HH:mm.");

        if (inicio >= fin)
            throw new InvalidOperationException("La hora de inicio debe ser menor que la hora de fin.");
    }

    private static bool HorariosSeCruzan(
        string inicioA,
        string finA,
        string inicioB,
        string finB)
    {
        TimeSpan aInicio = TimeSpan.Parse(inicioA);
        TimeSpan aFin = TimeSpan.Parse(finA);
        TimeSpan bInicio = TimeSpan.Parse(inicioB);
        TimeSpan bFin = TimeSpan.Parse(finB);

        return aInicio < bFin && bInicio < aFin;
    }

    private static BloqueoFranjaAsignaturaResponse ToResponse(BloqueoFranjaAsignatura bloqueo) => new()
    {
        IdBloqueo = bloqueo.IdBloqueo,
        IdAsignatura = bloqueo.IdAsignatura,
        CodigoAsignatura = bloqueo.Asignatura?.Codigo ?? string.Empty,
        NombreAsignatura = bloqueo.Asignatura?.Nombre ?? string.Empty,
        Periodo = bloqueo.Periodo,
        Dia = bloqueo.Dia,
        DiaNombre = ObtenerNombreDia(bloqueo.Dia),
        HoraInicio = bloqueo.HoraInicio,
        HoraFin = bloqueo.HoraFin,
        Motivo = bloqueo.Motivo,
        FechaCreacionUtc = bloqueo.FechaCreacionUtc
    };

    private static string ObtenerNombreDia(int dia)
    {
        return dia switch
        {
            1 => "Lunes",
            2 => "Martes",
            3 => "Miércoles",
            4 => "Jueves",
            5 => "Viernes",
            6 => "Sábado",
            _ => "Desconocido"
        };
    }
}