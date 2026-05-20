using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Infrastructure.Data;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace ApplicationSchedule.Infrastructure.Services;

/// <summary>
/// Servicio de exportación que genera un archivo Excel con los horarios confirmados.
/// Utiliza ClosedXML para construir el workbook y devuelve el contenido en bytes.
/// </summary>
public class HorarioExportService : IHorarioExportService
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Crea una instancia de <see cref="HorarioExportService"/> con el contexto inyectado.
    /// </summary>
    public HorarioExportService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Exporta horarios confirmados a un archivo Excel filtrando por parámetros opcionales.
    /// </summary>
    /// <param name="semestre">Semestre opcional para filtrar.</param>
    /// <param name="idDocente">Filtro por docente.</param>
    /// <param name="idAsignatura">Filtro por asignatura.</param>
    /// <param name="periodo">Periodo a exportar.</param>
    /// <returns>Array de bytes con el archivo Excel generado.</returns>
    public async Task<byte[]> ExportarHorariosAsync(int? semestre, string? idDocente, string? idAsignatura, string? periodo)
    {
        var query = _context.Set<ApplicationSchedule.Domain.Entities.Asignacion>()
            .Include(a => a.Docente)
            .Include(a => a.Asignatura)
            .Where(a => a.Estado == "Confirmada")
            .AsQueryable();

        if (!string.IsNullOrEmpty(periodo))
        {
            query = query.Where(a => a.Periodo == periodo);
        }

        if (semestre.HasValue)
        {
            query = query.Where(a => a.Asignatura!.Semestre == semestre.Value);
        }

        if (!string.IsNullOrEmpty(idDocente))
        {
            query = query.Where(a => a.IdDocente == idDocente);
        }

        if (!string.IsNullOrEmpty(idAsignatura))
        {
            query = query.Where(a => a.IdAsignatura == idAsignatura);
        }

        var asignaciones = await query.ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Horarios Confirmados");

        // Cabeceras
        worksheet.Cell(1, 1).Value = "Semestre";
        worksheet.Cell(1, 2).Value = "Asignatura";
        worksheet.Cell(1, 3).Value = "Docente";
        worksheet.Cell(1, 4).Value = "D�a";
        worksheet.Cell(1, 5).Value = "Hora Inicio";
        worksheet.Cell(1, 6).Value = "Hora Fin";
        worksheet.Cell(1, 7).Value = "Escenario";
        worksheet.Cell(1, 8).Value = "Periodo";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Datos
        int fila = 2;
        foreach (var asig in asignaciones)
        {
            worksheet.Cell(fila, 1).Value = asig.Asignatura?.Semestre.ToString() ?? "";
            worksheet.Cell(fila, 2).Value = asig.Asignatura?.Nombre ?? "";
            worksheet.Cell(fila, 3).Value = asig.Docente?.Nombre ?? "";
            worksheet.Cell(fila, 4).Value = ObtenerNombreDia((DayOfWeek)asig.Dia);
            worksheet.Cell(fila, 5).Value = asig.HoraInicio;
            worksheet.Cell(fila, 6).Value = asig.HoraFin;
            worksheet.Cell(fila, 7).Value = asig.Escenario;
            worksheet.Cell(fila, 8).Value = asig.Periodo;
            fila++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Convierte un valor <see cref="DayOfWeek"/> a su nombre en Español.
    /// </summary>
    private string ObtenerNombreDia(DayOfWeek dia)
    {
        return dia switch
        {
            DayOfWeek.Monday => "Lunes",
            DayOfWeek.Tuesday => "Martes",
            DayOfWeek.Wednesday => "Mi�rcoles",
            DayOfWeek.Thursday => "Jueves",
            DayOfWeek.Friday => "Viernes",
            DayOfWeek.Saturday => "S�bado",
            DayOfWeek.Sunday => "Domingo",
            _ => dia.ToString()
        };
    }
}
    