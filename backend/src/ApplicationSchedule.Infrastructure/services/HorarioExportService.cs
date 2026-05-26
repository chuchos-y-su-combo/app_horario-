using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Infrastructure.Data;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace ApplicationSchedule.Infrastructure.Services;

public class HorarioExportService : IHorarioExportService
{
    // Días completos (diurna Lun-Sáb = 6 días; nocturna Lun-Vie = 5 días)
    private static readonly string[] DiasSemana = ["Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado"];

    // Jornada diurna: 07:00 – 18:00 (11 slots de 1h)
    private const int HoraInicioDiurna = 7;
    private const int HoraFinDiurna = 18;

    // Jornada nocturna: 18:00 – 23:00 (5 slots de 1h)
    private const int HoraInicioNocturna = 18;
    private const int HoraFinNocturna = 23;

    // Sin filtro de jornada: rango completo 07:00 – 23:00
    private const int HoraInicioGeneral = 7;
    private const int HoraFinGeneral = 23;

    private readonly AppDbContext _context;

    public HorarioExportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> ExportarHorariosAsync(
        int? semestre,
        string? idDocente,
        string? idAsignatura,
        string? idPlan,
        string? periodo,
        bool porSemestre = false)
    {
        // ── Consulta base ────────────────────────────────────────────────────
        var query = _context.Set<Asignacion>()
            .Include(a => a.Docente)
            .Include(a => a.Asignatura)
                .ThenInclude(asig => asig!.PlanEstudio)
            .Where(a => a.Estado == "Propuesta" || a.Estado == "Confirmada")
            .AsQueryable();

        if (!string.IsNullOrEmpty(periodo))
            query = query.Where(a => a.Periodo == periodo);

        if (semestre.HasValue)
            query = query.Where(a => a.Asignatura!.Semestre == semestre.Value);

        if (!string.IsNullOrEmpty(idAsignatura))
            query = query.Where(a => a.IdAsignatura == idAsignatura);

        if (!string.IsNullOrEmpty(idPlan))
            query = query.Where(a => a.Asignatura!.IdPlan == idPlan);

        // No filtramos por idDocente aún porque "__ALL__" requiere iteración.
        bool todosDocentes = idDocente?.ToUpperInvariant() == "__ALL__";
        if (!todosDocentes && !string.IsNullOrEmpty(idDocente))
            query = query.Where(a => a.IdDocente == idDocente);

        var asignaciones = await query.ToListAsync();

        using var workbook = new XLWorkbook();

        if (todosDocentes)
        {
            // ── Modo: una hoja por docente ────────────────────────────────
            var grupos = asignaciones
                .GroupBy(a => new { a.IdDocente, Nombre = a.Docente?.Nombre ?? "Sin nombre" })
                .OrderBy(g => g.Key.Nombre);

            foreach (var grupo in grupos)
            {
                string nombreHoja = Truncar(grupo.Key.Nombre, 31);
                var jornada = DetectarJornada(grupo.ToList());
                CrearHojaHorario(workbook, nombreHoja, grupo.ToList(), jornada);
            }
        }
        else if (!string.IsNullOrEmpty(idPlan) && porSemestre)
        {
            // ── Modo: una hoja por semestre del plan ──────────────────────
            var plan = await _context.PlanesEstudio
                .Include(p => p.Asignaturas)
                .FirstOrDefaultAsync(p => p.IdPlan == idPlan);

            string jornada = plan?.Jornada ?? "Diurna";

            // Semestres que realmente tienen asignaciones
            var semestreGroups = asignaciones
                .GroupBy(a => a.Asignatura?.Semestre ?? 0)
                .Where(g => g.Key > 0)
                .OrderBy(g => g.Key);

            foreach (var grupo in semestreGroups)
            {
                string nombreHoja = $"Semestre {grupo.Key}";
                CrearHojaHorario(workbook, nombreHoja, grupo.ToList(), jornada);
            }
        }
        else if (!string.IsNullOrEmpty(idDocente) && !todosDocentes)
        {
            // ── Modo: horario individual de un docente ────────────────────
            var nombreDocente = asignaciones.FirstOrDefault()?.Docente?.Nombre ?? "Docente";
            var jornada = DetectarJornada(asignaciones);
            CrearHojaHorario(workbook, Truncar(nombreDocente, 31), asignaciones, jornada);
        }
        else if (!string.IsNullOrEmpty(idPlan) && !porSemestre)
        {
            // ── Modo: horario de un plan (hoja única) ─────────────────────
            var plan = await _context.PlanesEstudio.FindAsync(idPlan);
            string nombreHoja = Truncar(plan?.NombrePlan ?? "Plan", 31);
            string jornada = plan?.Jornada ?? "Diurna";
            CrearHojaHorario(workbook, nombreHoja, asignaciones, jornada);
        }
        else
        {
            // ── Modo: 4 hojas (una por escenario) ────────────────────────
            var escenarios = new[]
            {
                ("ING_DIURNA",    "Ingeniería Diurna",   "Diurna"),
                ("ING_NOCTURNA",  "Ingeniería Nocturna",  "Nocturna"),
                ("TAPSI_DIURNA",  "TAPSI Diurna",         "Diurna"),
                ("TAPSI_NOCTURNA","TAPSI Nocturna",        "Nocturna")
            };

            foreach (var (clave, nombre, jornada) in escenarios)
            {
                var subset = asignaciones.Where(a => a.Escenario == clave).ToList();
                CrearHojaHorario(workbook, nombre, subset, jornada);
            }
        }

        if (!workbook.Worksheets.Any())
            workbook.Worksheets.Add("Sin datos");

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Detecta la jornada mayoritaria de una lista de asignaciones.
    /// </summary>
    private static string DetectarJornada(List<Asignacion> asignaciones)
    {
        int nocturnas = asignaciones.Count(a =>
            a.Asignatura?.PlanEstudio?.Jornada?.ToLower() == "nocturna" ||
            (a.Escenario?.Contains("NOCTURNA", StringComparison.OrdinalIgnoreCase) == true));
        return nocturnas > asignaciones.Count / 2 ? "Nocturna" : "Diurna";
    }

    private static void CrearHojaHorario(
        IXLWorkbook workbook,
        string nombreHoja,
        List<Asignacion> asignaciones,
        string jornada)
    {
        bool esNocturna = jornada.Equals("Nocturna", StringComparison.OrdinalIgnoreCase);

        int horaInicio = esNocturna ? HoraInicioNocturna : HoraInicioDiurna;
        int horaFin    = esNocturna ? HoraFinNocturna    : HoraFinDiurna;
        int totalSlots = horaFin - horaInicio;

        // Nocturna: Lun-Vie (5 días). Diurna: Lun-Sáb (6 días).
        string[] dias = esNocturna
            ? ["Lunes", "Martes", "Miércoles", "Jueves", "Viernes"]
            : DiasSemana;
        int totalDias = dias.Length;

        var ws = workbook.Worksheets.Add(nombreHoja);

        // ----- Encabezados -----
        ws.Cell(1, 1).Value = "Hora";
        for (int d = 0; d < totalDias; d++)
            ws.Cell(1, d + 2).Value = dias[d];

        var headerRange = ws.Range(1, 1, 1, totalDias + 1);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#003087");
        headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // ----- Filas de franjas horarias -----
        for (int slot = 0; slot < totalSlots; slot++)
        {
            int row = slot + 2;
            int h = horaInicio + slot;
            ws.Cell(row, 1).Value = $"{h:00}:00 – {h + 1:00}:00";
            ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Row(row).Height = 45;

            ws.Cell(row, 1).Style.Fill.BackgroundColor =
                slot % 2 == 0 ? XLColor.FromHtml("#E8EFF7") : XLColor.FromHtml("#F5F5F5");

            for (int col = 2; col <= totalDias + 1; col++)
            {
                ws.Cell(row, col).Style.Fill.BackgroundColor =
                    slot % 2 == 0 ? XLColor.White : XLColor.FromHtml("#FAFAFA");
                ws.Cell(row, col).Style.Alignment.WrapText = true;
                ws.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }
        }

        // ----- Borde general -----
        var tableRange = ws.Range(1, 1, totalSlots + 1, totalDias + 1);
        tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
        tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        // ----- Colocar asignaciones -----
        foreach (var asig in asignaciones)
        {
            int hora = ObtenerHora(asig.HoraInicio);
            if (hora < horaInicio || hora >= horaFin) continue;

            // Dia 1=Lunes → col 2, etc. Para nocturna máx día 5 (Viernes)
            int col = asig.Dia + 1;
            if (col < 2 || col > totalDias + 1) continue;

            int row = hora - horaInicio + 2;

            // Calcular duración en slots (mín 1)
            int horaFinAsig = ObtenerHora(asig.HoraFin);
            int duracion = horaFinAsig > hora ? Math.Min(horaFinAsig - hora, totalSlots) : 1;
            int rowFin = Math.Min(row + duracion - 1, totalSlots + 1);

            // Combinar filas si duración > 1
            if (rowFin > row)
                ws.Range(row, col, rowFin, col).Merge();

            // Formato: "[Nombre] - [Código] | [Docente] | [Aula]"
            string nombre = asig.Asignatura?.Nombre ?? "—";
            string codigo = asig.Asignatura?.Codigo ?? "";
            string docente = asig.Docente?.Nombre ?? "—";
            string contenido = $"{nombre} - {codigo} | {docente} | Aula por asignar";

            var cell = ws.Cell(row, col);
            cell.Value = contenido;
            cell.Style.Alignment.WrapText = true;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Fill.BackgroundColor = esNocturna
                ? XLColor.FromHtml("#003087")
                : XLColor.FromHtml("#1A6BBF");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Font.FontSize = 8;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#003087");
        }

        // ----- Anchos de columna -----
        ws.Column(1).Width = 14;
        for (int col = 2; col <= totalDias + 1; col++)
            ws.Column(col).Width = 22;
    }

    private static int ObtenerHora(string horaStr)
    {
        if (string.IsNullOrEmpty(horaStr)) return -1;
        return int.TryParse(horaStr.Split(':')[0], out int h) ? h : -1;
    }

    private static string Truncar(string texto, int max) =>
        texto.Length > max ? texto[..max] : texto;
}
