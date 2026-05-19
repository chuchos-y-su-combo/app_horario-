namespace ApplicationSchedule.Application.DTOs.Horarios;

/// <summary>
/// Elemento individual usado en reportes de carga docente por hora/asignatura.
/// </summary>
public class ReporteDocenteItem
{
    public string IdDocente { get; set; } = string.Empty;

    public string NombreDocente { get; set; } = string.Empty;

    public string Identificacion { get; set; } = string.Empty;

    public string TipoContrato { get; set; } = string.Empty;

    public int MaxAsignaturas { get; set; }

    public int AsignaturasAsignadas { get; set; }

    /// <summary>
    /// Total de horas de clase semanales sumando todos los bloques del semestre.
    /// </summary>
    public double TotalHorasSemanales { get; set; }

    /// <summary>
    /// Horas contractuales esperadas según tipo de contrato.
    /// TC = 40 horas semanales. TP = 20 horas semanales.
    /// </summary>
    public double HorasContractuales { get; set; }

    /// <summary>
    /// Diferencia entre horas asignadas y horas contractuales.
    /// Positivo = supera la carga. Negativo = está por debajo.
    /// </summary>
    public double DiferenciaHoras { get; set; }

    /// <summary>
    /// Porcentaje de la carga contractual cubierta.
    /// </summary>
    public double PorcentajeCarga { get; set; }

    /// <summary>
    /// Sin asignaciones | Parcial | Completa | Excedida
    /// </summary>
    public string EstadoCarga { get; set; } = string.Empty;

    public string Semestre { get; set; } = string.Empty;

    public List<DetalleAsignaturaReporte> Asignaturas { get; set; } = new();
}

public class DetalleAsignaturaReporte
{
    public string NombreAsignatura { get; set; } = string.Empty;

    public string CodigoAsignatura { get; set; } = string.Empty;

    public string Escenario { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Horas semanales aportadas por esta asignatura (suma de todos sus bloques).
    /// </summary>
    public double HorasSemanales { get; set; }
}