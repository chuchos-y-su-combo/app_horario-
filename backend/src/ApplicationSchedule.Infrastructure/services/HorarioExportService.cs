using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Infrastructure.Data;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace ApplicationSchedule.Infrastructure.Services;

public class HorarioExportService : IHorarioExportService
{
    private static readonly string[] Dias = ["Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado"];

    // Hora de inicio y fin de la jornada (7:00 a 21:00 → 14 slots de 1h)
    private const int HoraInicio = 7;
    private const int HoraFin = 21;
    private const int TotalSlots = HoraFin - HoraInicio; // 14

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
        string? periodo)
    {
        var query = _context.Set<Asignacion>()
            .Include(a => a.Docente)
            .Include(a => a.Asignatura)
            .Where(a => a.Estado == "Propuesta" || a.Estado == "Confirmada")
            .AsQueryable();

        if (!string.IsNullOrEmpty(periodo))
            query = query.Where(a => a.Periodo == periodo);

        if (semestre.HasValue)
            query = query.Where(a => a.Asignatura!.Semestre == semestre.Value);

        if (!string.IsNullOrEmpty(idDocente))
            query = query.Where(a => a.IdDocente == idDocente);

        if (!string.IsNullOrEmpty(idAsignatura))
            query = query.Where(a => a.IdAsignatura == idAsignatura);

        if (!string.IsNullOrEmpty(idPlan))
            query = query.Where(a => a.Asignatura!.IdPlan == idPlan);

        var asignaciones = await query.ToListAsync();

        using var workbook = new XLWorkbook();

        if (!string.IsNullOrEmpty(idDocente))
        {
            var nombreDocente = asignaciones.FirstOrDefault()?.Docente?.Nombre ?? "Docente";
            CrearHojaHorario(workbook, Truncar(nombreDocente, 31), asignaciones);
        }
        else if (!string.IsNullOrEmpty(idPlan))
        {
            var plan = await _context.PlanesEstudio.FindAsync(idPlan);
            var nombreHoja = plan?.NombrePlan ?? "Plan";
            CrearHojaHorario(workbook, Truncar(nombreHoja, 31), asignaciones);
        }
        else
        {
            // 4 hojas — una por escenario
            var escenarios = new[]
            {
                ("ING_DIURNA",    "Ingeniería Diurna"),
                ("ING_NOCTURNA",  "Ingeniería Nocturna"),
                ("TAPSI_DIURNA",  "TAPSI Diurna"),
                ("TAPSI_NOCTURNA","TAPSI Nocturna")
            };

            foreach (var (clave, nombre) in escenarios)
            {
                var subset = asignaciones.Where(a => a.Escenario == clave).ToList();
                CrearHojaHorario(workbook, nombre, subset);
            }
        }

        if (!workbook.Worksheets.Any())
            workbook.Worksheets.Add("Sin datos");

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void CrearHojaHorario(IXLWorkbook workbook, string nombreHoja, List<Asignacion> asignaciones)
    {
        var ws = workbook.Worksheets.Add(nombreHoja);

        // ----- Encabezados -----
        ws.Cell(1, 1).Value = "Hora";
        for (int d = 0; d < Dias.Length; d++)
            ws.Cell(1, d + 2).Value = Dias[d];

        var headerRange = ws.Range(1, 1, 1, 7);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#003087");
        headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // ----- Filas de franjas horarias -----
        for (int slot = 0; slot < TotalSlots; slot++)
        {
            int row = slot + 2;
            ws.Cell(row, 1).Value = $"{HoraInicio + slot:00}:00";
            ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Row(row).Height = 52;

            // Fondo alterno en columna hora
            ws.Cell(row, 1).Style.Fill.BackgroundColor =
                slot % 2 == 0 ? XLColor.FromHtml("#E8EFF7") : XLColor.FromHtml("#F5F5F5");

            // Celdas de días: borde y fondo suave
            for (int col = 2; col <= 7; col++)
            {
                ws.Cell(row, col).Style.Fill.BackgroundColor =
                    slot % 2 == 0 ? XLColor.White : XLColor.FromHtml("#FAFAFA");
            }
        }

        // ----- Borde general -----
        var tableRange = ws.Range(1, 1, TotalSlots + 1, 7);
        tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
        tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        // ----- Colocar asignaciones -----
        foreach (var asig in asignaciones)
        {
            int hora = ObtenerHora(asig.HoraInicio);
            if (hora < HoraInicio || hora >= HoraFin) continue;

            int row = hora - HoraInicio + 2;   // fila en hoja (2 = primera franja = 7:00)
            int col = asig.Dia + 1;            // Dia 1=Lunes → col 2
            if (col < 2 || col > 7) continue;

            // Sesión de 2 horas: combinar 2 filas
            int rowFin = Math.Min(row + 1, TotalSlots + 1);
            if (rowFin > row)
            {
                ws.Range(row, col, rowFin, col).Merge();
            }

            string contenido =
                $"{asig.Asignatura?.Nombre ?? "—"}\n" +
                $"{asig.Asignatura?.Codigo ?? ""}\n" +
                $"{asig.Docente?.Nombre ?? "—"}\n" +
                "Aula por asignar";

            var cell = ws.Cell(row, col);
            cell.Value = contenido;
            cell.Style.Alignment.WrapText = true;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1A6BBF");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Font.FontSize = 8;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#003087");
        }

        // ----- Anchos de columna -----
        ws.Column(1).Width = 9;
        for (int col = 2; col <= 7; col++)
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
