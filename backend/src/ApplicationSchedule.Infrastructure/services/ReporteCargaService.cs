using ApplicationSchedule.Application.DTOs.Horarios;
using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ApplicationSchedule.Infrastructure.Services;

/// <summary>
/// Servicio que genera reportes de carga docente por semestre y por docente.
/// Calcula horas semanales, estado de carga y resumen de asignaturas.
/// </summary>
public class ReporteCargaService : IReporteCargaService
{
    private readonly AppDbContext _context;

    private const double HorasContractualesTC = 40.0;
    private const double HorasContractualesTP = 20.0;

    private static readonly string[] EstadosValidos =
        { "Propuesta", "AsignadaManual", "Confirmada" };

    /// <summary>
    /// Crea una nueva instancia del servicio con el contexto inyectado.
    /// </summary>
    public ReporteCargaService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Genera un reporte agregado de carga docente para el semestre indicado.
    /// </summary>
    public async Task<ReporteCargaDocenteResponse> GenerarReportePorSemestreAsync(
        string semestre,
        CancellationToken cancellationToken = default)
    {
        string semestreLimpio = semestre.Trim();

        List<Docente> docentes = await _context.Docentes
            .OrderBy(d => d.Nombre)
            .ToListAsync(cancellationToken);

        List<Asignacion> asignaciones = await _context.Asignaciones
            .Include(a => a.Asignatura)
            .Where(a => a.Periodo == semestreLimpio && EstadosValidos.Contains(a.Estado))
            .ToListAsync(cancellationToken);

        List<ReporteDocenteItem> items = docentes
            .Select(d => ConstruirItemDocente(
                d,
                semestreLimpio,
                asignaciones.Where(a => a.IdDocente == d.IdDocente).ToList()))
            .ToList();

        return new ReporteCargaDocenteResponse
        {
            Semestre = semestreLimpio,
            TotalDocentes = items.Count,
            DocentesConCargaCompleta = items.Count(i => i.EstadoCarga == "Completa"),
            DocentesConCargaParcial = items.Count(i => i.EstadoCarga == "Parcial"),
            DocentesConCargaExcedida = items.Count(i => i.EstadoCarga == "Excedida"),
            DocentesSinAsignaciones = items.Count(i => i.EstadoCarga == "Sin asignaciones"),
            Docentes = items
        };
    }

    /// <summary>
    /// Genera un reporte detallado para un docente en un semestre.
    /// </summary>
    public async Task<ReporteDocenteItem> GenerarReportePorDocenteAsync(
        string idDocente,
        string semestre,
        CancellationToken cancellationToken = default)
    {
        string semestreLimpio = semestre.Trim();

        Docente? docente = await _context.Docentes
            .FirstOrDefaultAsync(d => d.IdDocente == idDocente, cancellationToken);

        if (docente is null)
            throw new InvalidOperationException("Docente no encontrado.");

        List<Asignacion> asignaciones = await _context.Asignaciones
            .Include(a => a.Asignatura)
            .Where(a =>
                a.IdDocente == idDocente &&
                a.Periodo == semestreLimpio &&
                EstadosValidos.Contains(a.Estado))
            .ToListAsync(cancellationToken);

        return ConstruirItemDocente(docente, semestreLimpio, asignaciones);
    }

    // ── Privados ───────────────────────────────────────────────────────────

    private static ReporteDocenteItem ConstruirItemDocente(
        Docente docente,
        string semestre,
        List<Asignacion> asignaciones)
    {
        double horasContractuales = docente.TipoContrato == "TC"
            ? HorasContractualesTC
            : HorasContractualesTP;

        List<DetalleAsignaturaReporte> detalle = asignaciones
            .Where(a => a.Asignatura is not null)
            .GroupBy(a => new { a.IdAsignatura, a.Escenario, a.Estado })
            .Select(g => new DetalleAsignaturaReporte
            {
                NombreAsignatura = g.First().Asignatura!.Nombre,
                CodigoAsignatura = g.First().Asignatura!.Codigo,
                Escenario = g.Key.Escenario,
                Estado = g.Key.Estado,
                HorasSemanales = g.Sum(a => CalcularHoras(a.HoraInicio, a.HoraFin))
            })
            .OrderBy(d => d.NombreAsignatura)
            .ToList();

        int asignaturasDistintas = asignaciones
            .Select(a => a.IdAsignatura)
            .Distinct()
            .Count();

        double totalHoras = detalle.Sum(d => d.HorasSemanales);
        double diferencia = totalHoras - horasContractuales;
        double porcentaje = horasContractuales > 0
            ? Math.Round(totalHoras / horasContractuales * 100, 1)
            : 0;

        string estadoCarga = asignaturasDistintas == 0
            ? "Sin asignaciones"
            : totalHoras > horasContractuales
                ? "Excedida"
                : porcentaje >= 90
                    ? "Completa"
                    : "Parcial";

        return new ReporteDocenteItem
        {
            IdDocente = docente.IdDocente,
            NombreDocente = docente.Nombre,
            Identificacion = docente.Identificacion,
            TipoContrato = docente.TipoContrato,
            MaxAsignaturas = docente.MaxAsignaturas,
            AsignaturasAsignadas = asignaturasDistintas,
            TotalHorasSemanales = Math.Round(totalHoras, 2),
            HorasContractuales = horasContractuales,
            DiferenciaHoras = Math.Round(diferencia, 2),
            PorcentajeCarga = porcentaje,
            EstadoCarga = estadoCarga,
            Semestre = semestre,
            Asignaturas = detalle
        };
    }

    private static double CalcularHoras(string horaInicio, string horaFin)
    {
        if (string.IsNullOrEmpty(horaInicio) || string.IsNullOrEmpty(horaFin))
            return 0;

        if (!TimeSpan.TryParse(horaInicio, out TimeSpan inicio) ||
            !TimeSpan.TryParse(horaFin, out TimeSpan fin))
            return 0;

        double horas = (fin - inicio).TotalHours;
        return horas > 0 ? horas : 0;
    }
}