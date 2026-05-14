using ApplicationSchedule.Application.DTOs.Asignaciones;
using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ApplicationSchedule.Infrastructure.Services;

public class AsignacionService : IAsignacionService
{
    private readonly AppDbContext _context;

    public AsignacionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AsignacionResponse>> ObtenerTodasAsync()
    {
        List<Asignacion> asignaciones = await _context.Asignaciones
            .Include(a => a.Docente)
            .Include(a => a.Asignatura)
            .OrderBy(a => a.Periodo)
            .ThenBy(a => a.Docente!.Nombre)
            .ThenBy(a => a.Dia)
            .ThenBy(a => a.HoraInicio)
            .ToListAsync();

        List<AsignacionResponse> respuesta = new();

        foreach (Asignacion asignacion in asignaciones)
        {
            int asignaturasActuales = await ContarAsignaturasDistintasAsync(asignacion.IdDocente, asignacion.Periodo);
            respuesta.Add(ToResponse(asignacion, asignaturasActuales));
        }

        return respuesta;
    }

    public async Task<List<AsignacionResponse>> ObtenerPorDocenteAsync(string idDocente, string? periodo = null)
    {
        bool docenteExiste = await _context.Docentes.AnyAsync(d => d.IdDocente == idDocente);

        if (!docenteExiste)
            throw new InvalidOperationException("Docente no encontrado.");

        IQueryable<Asignacion> query = _context.Asignaciones
            .Include(a => a.Docente)
            .Include(a => a.Asignatura)
            .Where(a => a.IdDocente == idDocente);

        if (!string.IsNullOrWhiteSpace(periodo))
        {
            string periodoLimpio = periodo.Trim();
            query = query.Where(a => a.Periodo == periodoLimpio);
        }

        List<Asignacion> asignaciones = await query
            .OrderBy(a => a.Periodo)
            .ThenBy(a => a.Dia)
            .ThenBy(a => a.HoraInicio)
            .ToListAsync();

        List<AsignacionResponse> respuesta = new();

        foreach (Asignacion asignacion in asignaciones)
        {
            int asignaturasActuales = await ContarAsignaturasDistintasAsync(asignacion.IdDocente, asignacion.Periodo);
            respuesta.Add(ToResponse(asignacion, asignaturasActuales));
        }

        return respuesta;
    }

    public async Task<ResumenCargaDocenteResponse?> ObtenerResumenCargaDocenteAsync(string idDocente, string periodo)
    {
        Docente? docente = await _context.Docentes
            .FirstOrDefaultAsync(d => d.IdDocente == idDocente);

        if (docente is null)
            return null;

        string periodoLimpio = periodo.Trim();
        int asignaturasActuales = await ContarAsignaturasDistintasAsync(idDocente, periodoLimpio);

        return new ResumenCargaDocenteResponse
        {
            IdDocente = docente.IdDocente,
            NombreDocente = docente.Nombre,
            TipoContrato = docente.TipoContrato,
            MaxAsignaturas = docente.MaxAsignaturas,
            AsignaturasActuales = asignaturasActuales,
            PuedeAsignarMas = asignaturasActuales < docente.MaxAsignaturas,
            Periodo = periodoLimpio
        };
    }

    public async Task<AsignacionResponse> CrearAsync(CrearAsignacionRequest request)
    {
        string idDocente = request.IdDocente.Trim();
        string idAsignatura = request.IdAsignatura.Trim();
        string horaInicio = request.HoraInicio.Trim();
        string horaFin = request.HoraFin.Trim();
        string periodo = request.Periodo.Trim();

        if (string.CompareOrdinal(horaInicio, horaFin) >= 0)
            throw new InvalidOperationException("La hora de inicio debe ser menor que la hora de fin.");

        Docente? docente = await _context.Docentes
            .FirstOrDefaultAsync(d => d.IdDocente == idDocente);

        if (docente is null)
            throw new InvalidOperationException("Docente no encontrado.");

        Asignatura? asignatura = await _context.Asignaturas
            .FirstOrDefaultAsync(a => a.IdAsignatura == idAsignatura);

        if (asignatura is null)
            throw new InvalidOperationException("Asignatura no encontrada.");

        ValidarContratoDocente(docente);

        bool docenteTieneCurriculoCargado = await _context.DocentesHabilitados
            .AnyAsync(dh => dh.IdDocente == docente.IdDocente);

        if (docenteTieneCurriculoCargado)
        {
            bool docentePuedeDictarAsignatura = await _context.DocentesHabilitados
                .AnyAsync(dh =>
                    dh.IdDocente == docente.IdDocente &&
                    dh.IdAsignatura == asignatura.IdAsignatura);

            if (!docentePuedeDictarAsignatura)
                throw new InvalidOperationException(
                    $"El docente {docente.Nombre} no está habilitado por currículo para dictar la asignatura {asignatura.Nombre}.");
        }

        int asignaturasActuales = await ContarAsignaturasDistintasAsync(idDocente, periodo);

        bool asignaturaYaAsignadaEnPeriodo = await _context.Asignaciones
            .AnyAsync(a =>
                a.IdDocente == idDocente &&
                a.IdAsignatura == idAsignatura &&
                a.Periodo == periodo);

        if (!asignaturaYaAsignadaEnPeriodo && asignaturasActuales >= docente.MaxAsignaturas)
            throw new InvalidOperationException(
                $"El docente con contrato {docente.TipoContrato} ya alcanzó el límite de {docente.MaxAsignaturas} asignaturas para el periodo {periodo}.");

        bool bloqueDuplicado = await _context.Asignaciones
            .AnyAsync(a =>
                a.IdDocente == idDocente &&
                a.IdAsignatura == idAsignatura &&
                a.Dia == request.Dia &&
                a.HoraInicio == horaInicio &&
                a.HoraFin == horaFin &&
                a.Periodo == periodo);

        if (bloqueDuplicado)
            throw new InvalidOperationException("Esta asignación ya existe para el mismo docente, asignatura, día, horario y periodo.");

        var asignacion = new Asignacion
        {
            IdAsignacion = Guid.NewGuid().ToString(),
            IdDocente = docente.IdDocente,
            IdAsignatura = asignatura.IdAsignatura,
            Dia = request.Dia,
            HoraInicio = horaInicio,
            HoraFin = horaFin,
            Periodo = periodo,
            Estado = "Propuesta"
        };

        _context.Asignaciones.Add(asignacion);
        await _context.SaveChangesAsync();

        asignacion.Docente = docente;
        asignacion.Asignatura = asignatura;

        int asignaturasLuegoDeAsignar = asignaturaYaAsignadaEnPeriodo
            ? asignaturasActuales
            : asignaturasActuales + 1;

        return ToResponse(asignacion, asignaturasLuegoDeAsignar);
    }

    public async Task<bool> EliminarAsync(string idAsignacion)
    {
        Asignacion? asignacion = await _context.Asignaciones
            .FirstOrDefaultAsync(a => a.IdAsignacion == idAsignacion);

        if (asignacion is null)
            return false;

        _context.Asignaciones.Remove(asignacion);
        await _context.SaveChangesAsync();

        return true;
    }

    // ── Issue #10 ──────────────────────────────────────────────────────────

    public async Task<AsignacionResponse> AsignarManualmenteAsync(AsignarAsignaturaManualRequest request)
    {
        string idDocente = request.IdDocente.Trim();
        string idAsignatura = request.IdAsignatura.Trim();
        string periodo = request.Periodo.Trim();

        Docente? docente = await _context.Docentes
            .FirstOrDefaultAsync(d => d.IdDocente == idDocente);

        if (docente is null)
            throw new InvalidOperationException("Docente no encontrado.");

        Asignatura? asignatura = await _context.Asignaturas
            .FirstOrDefaultAsync(a => a.IdAsignatura == idAsignatura);

        if (asignatura is null)
            throw new InvalidOperationException("Asignatura no encontrada.");

        ValidarContratoDocente(docente);

        if (!request.ForzarSinCurriculo)
        {
            bool tieneCurriculo = await _context.DocentesHabilitados
                .AnyAsync(dh => dh.IdDocente == idDocente);

            if (tieneCurriculo)
            {
                bool habilitado = await _context.DocentesHabilitados
                    .AnyAsync(dh => dh.IdDocente == idDocente && dh.IdAsignatura == idAsignatura);

                if (!habilitado)
                    throw new InvalidOperationException(
                        $"El docente {docente.Nombre} no está habilitado por currículo para dictar " +
                        $"'{asignatura.Nombre}'. Use ForzarSinCurriculo = true para sobrescribir.");
            }
        }

        int asignaturasActuales = await ContarAsignaturasDistintasAsync(idDocente, periodo);

        bool yaAsignada = await _context.Asignaciones
            .AnyAsync(a => a.IdDocente == idDocente && a.IdAsignatura == idAsignatura && a.Periodo == periodo);

        if (yaAsignada)
            throw new InvalidOperationException(
                $"La asignatura '{asignatura.Nombre}' ya está asignada al docente en el periodo {periodo}.");

        if (asignaturasActuales >= docente.MaxAsignaturas)
            throw new InvalidOperationException(
                $"El docente ya alcanzó el límite de {docente.MaxAsignaturas} asignaturas para el periodo {periodo}.");

        var asignacion = new Asignacion
        {
            IdAsignacion = Guid.NewGuid().ToString(),
            IdDocente = idDocente,
            IdAsignatura = idAsignatura,
            Dia = 0,
            HoraInicio = string.Empty,
            HoraFin = string.Empty,
            Periodo = periodo,
            Estado = "AsignadaManual"
        };

        _context.Asignaciones.Add(asignacion);
        await _context.SaveChangesAsync();

        asignacion.Docente = docente;
        asignacion.Asignatura = asignatura;

        return ToResponse(asignacion, asignaturasActuales + 1);
    }

    public async Task<List<AsignaturaDisponibleParaDocenteResponse>> ObtenerAsignaturasDisponiblesParaDocenteAsync(
        string idDocente,
        string periodo)
    {
        bool docenteExiste = await _context.Docentes.AnyAsync(d => d.IdDocente == idDocente);

        if (!docenteExiste)
            throw new InvalidOperationException("Docente no encontrado.");

        string periodoLimpio = periodo.Trim();

        List<string> yaAsignadas = await _context.Asignaciones
            .Where(a => a.IdDocente == idDocente && a.Periodo == periodoLimpio)
            .Select(a => a.IdAsignatura)
            .Distinct()
            .ToListAsync();

        List<string> habilitadasPorCurriculo = await _context.DocentesHabilitados
            .Where(dh => dh.IdDocente == idDocente)
            .Select(dh => dh.IdAsignatura)
            .ToListAsync();

        List<Asignatura> todas = await _context.Asignaturas
            .OrderBy(a => a.Semestre)
            .ThenBy(a => a.Nombre)
            .ToListAsync();

        return todas.Select(a => new AsignaturaDisponibleParaDocenteResponse
        {
            IdAsignatura = a.IdAsignatura,
            Codigo = a.Codigo,
            Nombre = a.Nombre,
            Creditos = a.Creditos,
            Semestre = a.Semestre,
            HabilitadaPorCurriculo = habilitadasPorCurriculo.Contains(a.IdAsignatura),
            YaAsignadaEnPeriodo = yaAsignadas.Contains(a.IdAsignatura)
        }).ToList();
    }


    private async Task<int> ContarAsignaturasDistintasAsync(string idDocente, string periodo)
    {
        return await _context.Asignaciones
            .Where(a => a.IdDocente == idDocente && a.Periodo == periodo)
            .Select(a => a.IdAsignatura)
            .Distinct()
            .CountAsync();
    }

    private static void ValidarContratoDocente(Docente docente)
    {
        if (docente.TipoContrato == "TC" && docente.MaxAsignaturas == 5) return;
        if (docente.TipoContrato == "TP" && docente.MaxAsignaturas == 3) return;

        throw new InvalidOperationException(
            "La configuración del contrato docente no es válida. TC debe tener máximo 5 asignaturas y TP máximo 3.");
    }

    private static AsignacionResponse ToResponse(Asignacion asignacion, int asignaturasActuales) => new()
    {
        IdAsignacion = asignacion.IdAsignacion,
        IdDocente = asignacion.IdDocente,
        NombreDocente = asignacion.Docente?.Nombre ?? string.Empty,
        TipoContrato = asignacion.Docente?.TipoContrato ?? string.Empty,
        MaxAsignaturas = asignacion.Docente?.MaxAsignaturas ?? 0,
        AsignaturasActuales = asignaturasActuales,
        IdAsignatura = asignacion.IdAsignatura,
        CodigoAsignatura = asignacion.Asignatura?.Codigo ?? string.Empty,
        NombreAsignatura = asignacion.Asignatura?.Nombre ?? string.Empty,
        Dia = asignacion.Dia,
        HoraInicio = asignacion.HoraInicio,
        HoraFin = asignacion.HoraFin,
        Periodo = asignacion.Periodo,
        Estado = asignacion.Estado
    };
}