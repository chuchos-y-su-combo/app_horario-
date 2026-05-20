namespace ApplicationSchedule.Application.DTOs.Horarios;

/// <summary>
/// DTO que representa el reporte de carga docente consolidado.
/// Incluye resumenes por docente y totales por escenario/periodo.
/// </summary>
public class ReporteCargaDocenteResponse
{
    public string Semestre { get; set; } = string.Empty;

    public int TotalDocentes { get; set; }

    public int DocentesConCargaCompleta { get; set; }

    public int DocentesConCargaParcial { get; set; }

    public int DocentesConCargaExcedida { get; set; }

    public int DocentesSinAsignaciones { get; set; }

    public List<ReporteDocenteItem> Docentes { get; set; } = new();
}