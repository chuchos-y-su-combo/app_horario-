namespace ApplicationSchedule.Application.DTOs.Horarios;

/// <summary>
/// DTO que representa el calendario semanal de un docente con sus asignaciones.
/// </summary>
public class CalendarioDocenteResponse
{
    public string IdDocente { get; set; } = string.Empty;

    public string NombreDocente { get; set; } = string.Empty;

    public string Identificacion { get; set; } = string.Empty;

    public string TipoContrato { get; set; } = string.Empty;

    public int MaxAsignaturas { get; set; }

    public string Semestre { get; set; } = string.Empty;

    /// <summary>
    /// Total de asignaturas distintas asignadas al docente en el semestre.
    /// </summary>
    public int TotalAsignaturas { get; set; }

    /// <summary>
    /// Total de horas semanales sumando todos los bloques con horario definido.
    /// </summary>
    public double TotalHorasSemanales { get; set; }

    /// <summary>
    /// Columnas del calendario: Lunes a Sábado (días 1-6).
    /// </summary>
    public List<DiaSemanaCalendario> Dias { get; set; } = new();
}
