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
            .OrderByDescending(a => a.Periodo)
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

    public async Task<List<string>> ObtenerPeriodosHistoricosAsync()
    {
        return await _context.Asignaciones
            .Select(a => a.Periodo)
            .Distinct()
            .OrderByDescending(p => p)
            .ToListAsync();
    }

    public async Task<List<AsignacionResponse>> ObtenerFiltradasAsync(int? semestre, string? idDocente, string? idAsignatura, string? periodo)
    {
        var query = _context.Set<Asignacion>()
            .Include(a => a.Docente)
            .Include(a => a.Asignatura)
            .AsQueryable();

        if (semestre.HasValue)
            query = query.Where(a => a.Asignatura!.Semestre == semestre.Value);

        if (!string.IsNullOrWhiteSpace(idDocente))
            query = query.Where(a => a.IdDocente == idDocente);

        if (!string.IsNullOrWhiteSpace(idAsignatura))
            query = query.Where(a => a.IdAsignatura == idAsignatura);

        if (!string.IsNullOrWhiteSpace(periodo))
            query = query.Where(a => a.Periodo == periodo);

        List<Asignacion> asignaciones = await query
            .OrderByDescending(a => a.Periodo)
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
    public async Task<List<AsignacionResponse>> ObtenerPropuestasPorPeriodoAsync(string periodo)
    {
        string periodoLimpio = periodo.Trim();

        List<Asignacion> propuestas = await _context.Asignaciones
            .Include(a => a.Docente)
            .Include(a => a.Asignatura)
            .Where(a => a.Periodo == periodoLimpio &&
                        (a.Estado == "Propuesta" || a.Estado == "AsignadaManual"))
            .OrderBy(a => a.Escenario)
            .ThenBy(a => a.Docente!.Nombre)
            .ThenBy(a => a.Dia)
            .ThenBy(a => a.HoraInicio)
            .ToListAsync();

        List<AsignacionResponse> respuesta = new();

        foreach (Asignacion asignacion in propuestas)
        {
            int asignaturasActuales = await ContarAsignaturasDistintasAsync(
                asignacion.IdDocente, asignacion.Periodo);

            respuesta.Add(ToResponse(asignacion, asignaturasActuales));
        }

        return respuesta;
    }

    public async Task<AsignacionResponse> AjustarAsync(string idAsignacion, AjustarAsignacionRequest request)
    {
        Asignacion? asignacion = await _context.Asignaciones
            .Include(a => a.Docente)
            .Include(a => a.Asignatura)
            .FirstOrDefaultAsync(a => a.IdAsignacion == idAsignacion);

        if (asignacion is null)
            throw new InvalidOperationException("Asignación no encontrada.");

        if (asignacion.Estado == "Confirmada")
            throw new InvalidOperationException(
                "No se puede ajustar una asignación que ya fue confirmada.");

        if (asignacion.Estado == "Cancelada")
            throw new InvalidOperationException(
                "No se puede ajustar una asignación cancelada.");

        // Cambio de docente
        if (!string.IsNullOrWhiteSpace(request.IdDocente) &&
            request.IdDocente != asignacion.IdDocente)
        {
            Docente? nuevoDocente = await _context.Docentes
                .FirstOrDefaultAsync(d => d.IdDocente == request.IdDocente);

            if (nuevoDocente is null)
                throw new InvalidOperationException("El nuevo docente no fue encontrado.");

            ValidarContratoDocente(nuevoDocente);

            string periodo = request.Periodo?.Trim() ?? asignacion.Periodo;
            string idAsignatura = request.IdAsignatura ?? asignacion.IdAsignatura;

            int cargaNuevoDocente = await ContarAsignaturasDistintasAsync(request.IdDocente, periodo);

            bool yaAsignadaANuevoDocente = await _context.Asignaciones
                .AnyAsync(a => a.IdDocente == request.IdDocente &&
                               a.IdAsignatura == idAsignatura &&
                               a.Periodo == periodo &&
                               a.IdAsignacion != idAsignacion);

            if (!yaAsignadaANuevoDocente && cargaNuevoDocente >= nuevoDocente.MaxAsignaturas)
                throw new InvalidOperationException(
                    $"El nuevo docente ya alcanzó el límite de {nuevoDocente.MaxAsignaturas} " +
                    $"asignaturas para el periodo {periodo}.");

            asignacion.IdDocente = request.IdDocente;
            asignacion.Docente = nuevoDocente;
        }

        // Cambio de asignatura
        if (!string.IsNullOrWhiteSpace(request.IdAsignatura) &&
            request.IdAsignatura != asignacion.IdAsignatura)
        {
            Asignatura? nuevaAsignatura = await _context.Asignaturas
                .FirstOrDefaultAsync(a => a.IdAsignatura == request.IdAsignatura);

            if (nuevaAsignatura is null)
                throw new InvalidOperationException("La nueva asignatura no fue encontrada.");

            asignacion.IdAsignatura = request.IdAsignatura;
            asignacion.Asignatura = nuevaAsignatura;
        }

        // Cambio de bloque horario
        if (request.Dia.HasValue)
            asignacion.Dia = request.Dia.Value;

        if (!string.IsNullOrWhiteSpace(request.HoraInicio))
            asignacion.HoraInicio = request.HoraInicio.Trim();

        if (!string.IsNullOrWhiteSpace(request.HoraFin))
            asignacion.HoraFin = request.HoraFin.Trim();

        if (!string.IsNullOrWhiteSpace(request.Periodo))
            asignacion.Periodo = request.Periodo.Trim();

        // Validar que hora inicio < hora fin si ambas están definidas
        if (!string.IsNullOrEmpty(asignacion.HoraInicio) &&
            !string.IsNullOrEmpty(asignacion.HoraFin) &&
            string.CompareOrdinal(asignacion.HoraInicio, asignacion.HoraFin) >= 0)
        {
            throw new InvalidOperationException(
                "La hora de inicio debe ser menor que la hora de fin.");
        }

        await _context.SaveChangesAsync();

        int asignaturasActuales = await ContarAsignaturasDistintasAsync(
            asignacion.IdDocente, asignacion.Periodo);

        return ToResponse(asignacion, asignaturasActuales);
    }

    public async Task<ResultadoConfirmacionResponse> ConfirmarAsync(ConfirmarAsignacionesRequest request)
    {
        ResultadoConfirmacionResponse resultado = new();

        foreach (string id in request.IdsAsignacion)
        {
            Asignacion? asignacion = await _context.Asignaciones
                .FirstOrDefaultAsync(a => a.IdAsignacion == id);

            if (asignacion is null)
            {
                resultado.Fallidas++;
                resultado.Errores.Add(new ResultadoFallidoItem
                {
                    IdAsignacion = id,
                    Motivo = "Asignación no encontrada."
                });
                continue;
            }

            if (asignacion.Estado == "Confirmada")
            {
                resultado.Fallidas++;
                resultado.Errores.Add(new ResultadoFallidoItem
                {
                    IdAsignacion = id,
                    Motivo = "La asignación ya estaba confirmada."
                });
                continue;
            }

            if (asignacion.Estado == "Cancelada")
            {
                resultado.Fallidas++;
                resultado.Errores.Add(new ResultadoFallidoItem
                {
                    IdAsignacion = id,
                    Motivo = "No se puede confirmar una asignación cancelada."
                });
                continue;
            }

            asignacion.Estado = "Confirmada";
            resultado.Confirmadas++;
            resultado.IdsConfirmadas.Add(id);
        }

        if (resultado.Confirmadas > 0)
            await _context.SaveChangesAsync();

        return resultado;
    }

    public async Task<AsignacionResponse> CancelarAsync(string idAsignacion)
    {
        Asignacion? asignacion = await _context.Asignaciones
            .Include(a => a.Docente)
            .Include(a => a.Asignatura)
            .FirstOrDefaultAsync(a => a.IdAsignacion == idAsignacion);

        if (asignacion is null)
            throw new InvalidOperationException("Asignación no encontrada.");

        if (asignacion.Estado == "Confirmada")
            throw new InvalidOperationException(
                "No se puede cancelar una asignación ya confirmada. Elimínela si es necesario.");

        if (asignacion.Estado == "Cancelada")
            throw new InvalidOperationException("La asignación ya está cancelada.");

        asignacion.Estado = "Cancelada";
        await _context.SaveChangesAsync();

        int asignaturasActuales = await ContarAsignaturasDistintasAsync(
            asignacion.IdDocente, asignacion.Periodo);

        return ToResponse(asignacion, asignaturasActuales);
    }
    public async Task<AsignacionResponse> AsignarDiaAsync(
    string idAsignacion,
    AsignarDiaRequest request)
    {
        Asignacion? asignacion = await _context.Asignaciones
            .Include(a => a.Docente)
            .Include(a => a.Asignatura)
            .FirstOrDefaultAsync(a => a.IdAsignacion == idAsignacion);

        if (asignacion is null)
            throw new InvalidOperationException("Asignación no encontrada.");

        if (asignacion.Estado == "Confirmada")
            throw new InvalidOperationException(
                "No se puede modificar el día de una asignación ya confirmada.");

        if (asignacion.Estado == "Cancelada")
            throw new InvalidOperationException(
                "No se puede modificar el día de una asignación cancelada.");

        // Si se envía una hora, ambas deben estar presentes
        bool enviandoHoras = !string.IsNullOrWhiteSpace(request.HoraInicio) ||
                             !string.IsNullOrWhiteSpace(request.HoraFin);

        if (enviandoHoras)
        {
            if (string.IsNullOrWhiteSpace(request.HoraInicio))
                throw new InvalidOperationException(
                    "Si se especifica hora de fin, también debe especificarse hora de inicio.");

            if (string.IsNullOrWhiteSpace(request.HoraFin))
                throw new InvalidOperationException(
                    "Si se especifica hora de inicio, también debe especificarse hora de fin.");

            if (string.CompareOrdinal(request.HoraInicio, request.HoraFin) >= 0)
                throw new InvalidOperationException(
                    "La hora de inicio debe ser menor que la hora de fin.");
        }

        asignacion.Dia = request.Dia;

        if (enviandoHoras)
        {
            asignacion.HoraInicio = request.HoraInicio!.Trim();
            asignacion.HoraFin = request.HoraFin!.Trim();
        }

        await _context.SaveChangesAsync();

        int asignaturasActuales = await ContarAsignaturasDistintasAsync(
            asignacion.IdDocente, asignacion.Periodo);

        return ToResponse(asignacion, asignaturasActuales);
    }
}