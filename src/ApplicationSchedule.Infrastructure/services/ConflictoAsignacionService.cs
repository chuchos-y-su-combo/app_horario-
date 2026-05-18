using ApplicationSchedule.Application.DTOs.Asignaciones;
using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ApplicationSchedule.Infrastructure.Services;

public class ConflictoAsignacionService : IConflictoAsignacionService
{
    private readonly AppDbContext _context;

    private static readonly string[] EstadosActivos =
        { "Propuesta", "AsignadaManual", "Confirmada" };

    public ConflictoAsignacionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ConflictoAsignacionResponse> AnalizarConflictosAsync(
        string semestre,
        CancellationToken cancellationToken = default)
    {
        string semestreLimpio = semestre.Trim();

        List<Asignacion> asignaciones = await _context.Asignaciones
            .Include(a => a.Docente)
            .Include(a => a.Asignatura)
            .Where(a => a.Periodo == semestreLimpio && EstadosActivos.Contains(a.Estado))
            .ToListAsync(cancellationToken);

        List<Asignatura> todasLasAsignaturas = await _context.Asignaturas
            .ToListAsync(cancellationToken);

        List<BloqueoFranjaAsignatura> bloqueosFranja = await _context.BloqueosFranjaAsignatura
            .Include(b => b.Asignatura)
            .Where(b => b.Periodo == semestreLimpio)
            .ToListAsync(cancellationToken);

        List<AlertaConflicto> conflictos = new();

        conflictos.AddRange(DetectarCrucesHorarios(asignaciones));
        conflictos.AddRange(DetectarExcesoDeCarga(asignaciones));
        conflictos.AddRange(DetectarAsignaturasSinDocente(todasLasAsignaturas, asignaciones, semestreLimpio));
        conflictos.AddRange(DetectarDocentesConAsignaturaSinHorario(asignaciones));
        conflictos.AddRange(DetectarAsignacionesEnFranjaBloqueada(asignaciones, bloqueosFranja));

        return new ConflictoAsignacionResponse
        {
            Semestre = semestreLimpio,
            TotalConflictos = conflictos.Count,
            Conflictos = conflictos
        };
    }

    // ── Detectores ─────────────────────────────────────────────────────────

    private static List<AlertaConflicto> DetectarCrucesHorarios(List<Asignacion> asignaciones)
    {
        List<AlertaConflicto> alertas = new();

        // Solo se pueden cruzar asignaciones del mismo docente, mismo día, con horario definido
        List<Asignacion> conHorario = asignaciones
            .Where(a => !string.IsNullOrEmpty(a.HoraInicio) && !string.IsNullOrEmpty(a.HoraFin))
            .ToList();

        List<IGrouping<(string, int), Asignacion>> grupos = conHorario
            .GroupBy(a => (a.IdDocente, a.Dia))
            .Where(g => g.Count() > 1)
            .ToList();

        foreach (IGrouping<(string, int), Asignacion> grupo in grupos)
        {
            List<Asignacion> bloques = grupo.ToList();

            for (int i = 0; i < bloques.Count; i++)
            {
                for (int j = i + 1; j < bloques.Count; j++)
                {
                    Asignacion a = bloques[i];
                    Asignacion b = bloques[j];

                    if (HorariosSeSolapan(a.HoraInicio, a.HoraFin, b.HoraInicio, b.HoraFin))
                    {
                        alertas.Add(new AlertaConflicto
                        {
                            TipoConflicto = "CruceHorario",
                            Severidad = "Error",
                            Descripcion =
                                $"El docente '{a.Docente?.Nombre}' tiene cruce horario el día {a.Dia} " +
                                $"entre '{a.Asignatura?.Nombre}' ({a.HoraInicio}-{a.HoraFin}) " +
                                $"y '{b.Asignatura?.Nombre}' ({b.HoraInicio}-{b.HoraFin}).",
                            IdDocente = a.IdDocente,
                            NombreDocente = a.Docente?.Nombre,
                            IdAsignacion1 = a.IdAsignacion,
                            IdAsignacion2 = b.IdAsignacion,
                            DetalleHorario = $"Día {a.Dia}: {a.HoraInicio}-{a.HoraFin} vs {b.HoraInicio}-{b.HoraFin}"
                        });
                    }
                }
            }
        }

        return alertas;
    }

    private static List<AlertaConflicto> DetectarExcesoDeCarga(List<Asignacion> asignaciones)
    {
        List<AlertaConflicto> alertas = new();

        List<IGrouping<string, Asignacion>> porDocente = asignaciones
            .GroupBy(a => a.IdDocente)
            .ToList();

        foreach (IGrouping<string, Asignacion> grupo in porDocente)
        {
            Docente? docente = grupo.First().Docente;

            if (docente is null) continue;

            int asignaturasDistintas = grupo
                .Select(a => a.IdAsignatura)
                .Distinct()
                .Count();

            if (asignaturasDistintas > docente.MaxAsignaturas)
            {
                alertas.Add(new AlertaConflicto
                {
                    TipoConflicto = "ExcesoCarga",
                    Severidad = "Error",
                    Descripcion =
                        $"El docente '{docente.Nombre}' ({docente.TipoContrato}) tiene " +
                        $"{asignaturasDistintas} asignaturas asignadas, superando su límite de {docente.MaxAsignaturas}.",
                    IdDocente = docente.IdDocente,
                    NombreDocente = docente.Nombre
                });
            }
        }

        return alertas;
    }

    private static List<AlertaConflicto> DetectarAsignaturasSinDocente(
        List<Asignatura> todasLasAsignaturas,
        List<Asignacion> asignaciones,
        string semestre)
    {
        List<AlertaConflicto> alertas = new();

        HashSet<string> asignaturasConDocente = asignaciones
            .Select(a => a.IdAsignatura)
            .ToHashSet();

        foreach (Asignatura asignatura in todasLasAsignaturas)
        {
            if (!asignaturasConDocente.Contains(asignatura.IdAsignatura))
            {
                alertas.Add(new AlertaConflicto
                {
                    TipoConflicto = "AsignaturaSinDocente",
                    Severidad = "Advertencia",
                    Descripcion =
                        $"La asignatura '{asignatura.Nombre}' ({asignatura.Codigo}) " +
                        $"no tiene ningún docente asignado en el semestre {semestre}.",
                    NombreAsignatura = asignatura.Nombre
                });
            }
        }

        return alertas;
    }

    private static List<AlertaConflicto> DetectarDocentesConAsignaturaSinHorario(
        List<Asignacion> asignaciones)
    {
        List<AlertaConflicto> alertas = new();

        List<Asignacion> sinHorario = asignaciones
            .Where(a => string.IsNullOrEmpty(a.HoraInicio) || string.IsNullOrEmpty(a.HoraFin))
            .ToList();

        foreach (Asignacion asignacion in sinHorario)
        {
            alertas.Add(new AlertaConflicto
            {
                TipoConflicto = "DocenteSinHorario",
                Severidad = "Advertencia",
                Descripcion =
                    $"El docente '{asignacion.Docente?.Nombre}' tiene la asignatura " +
                    $"'{asignacion.Asignatura?.Nombre}' sin bloque horario definido (estado: {asignacion.Estado}).",
                IdDocente = asignacion.IdDocente,
                NombreDocente = asignacion.Docente?.Nombre,
                IdAsignacion1 = asignacion.IdAsignacion,
                NombreAsignatura = asignacion.Asignatura?.Nombre
            });
        }

        return alertas;
    }

    private static List<AlertaConflicto> DetectarAsignacionesEnFranjaBloqueada(
        List<Asignacion> asignaciones,
        List<BloqueoFranjaAsignatura> bloqueosFranja)
    {
        List<AlertaConflicto> alertas = new();

        List<Asignacion> conHorario = asignaciones
            .Where(a =>
                a.Dia >= 1 &&
                !string.IsNullOrWhiteSpace(a.HoraInicio) &&
                !string.IsNullOrWhiteSpace(a.HoraFin))
            .ToList();

        foreach (Asignacion asignacion in conHorario)
        {
            BloqueoFranjaAsignatura? bloqueo = bloqueosFranja.FirstOrDefault(b =>
                b.IdAsignatura == asignacion.IdAsignatura &&
                b.Periodo == asignacion.Periodo &&
                b.Dia == asignacion.Dia &&
                HorariosSeSolapan(asignacion.HoraInicio, asignacion.HoraFin, b.HoraInicio, b.HoraFin));

            if (bloqueo is null)
                continue;

            string motivo = string.IsNullOrWhiteSpace(bloqueo.Motivo)
                ? "sin motivo registrado"
                : bloqueo.Motivo;

            alertas.Add(new AlertaConflicto
            {
                TipoConflicto = "FranjaBloqueadaAsignatura",
                Severidad = "Error",
                Descripcion =
                    $"La asignatura '{asignacion.Asignatura?.Nombre}' tiene una asignación " +
                    $"en una franja bloqueada: día {asignacion.Dia}, " +
                    $"{asignacion.HoraInicio}-{asignacion.HoraFin}. " +
                    $"Bloqueo registrado: {bloqueo.HoraInicio}-{bloqueo.HoraFin}. Motivo: {motivo}.",
                IdDocente = asignacion.IdDocente,
                NombreDocente = asignacion.Docente?.Nombre,
                IdAsignacion1 = asignacion.IdAsignacion,
                NombreAsignatura = asignacion.Asignatura?.Nombre,
                DetalleHorario = $"Día {asignacion.Dia}: {asignacion.HoraInicio}-{asignacion.HoraFin} bloqueado por {bloqueo.HoraInicio}-{bloqueo.HoraFin}"
            });
        }

        return alertas;
    }
    private static bool HorariosSeSolapan(
        string inicioA, string finA,
        string inicioB, string finB)
    {
        if (!TimeSpan.TryParse(inicioA, out TimeSpan a1) ||
            !TimeSpan.TryParse(finA, out TimeSpan a2) ||
            !TimeSpan.TryParse(inicioB, out TimeSpan b1) ||
            !TimeSpan.TryParse(finB, out TimeSpan b2))
            return false;

        return a1 < b2 && b1 < a2;
    }
}