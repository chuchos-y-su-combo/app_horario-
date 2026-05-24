namespace ApplicationSchedule.Domain.Entities;

/// <summary>
/// Representa la asignación de una asignatura a un docente en un día y franja horaria.
/// Esta entidad se utiliza tanto para propuestas como para asignaciones confirmadas.
/// </summary>
public class Asignacion
{
    /// <summary>
    /// Identificador único de la asignación.
    /// </summary>
    public string IdAsignacion { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Identificador del docente asignado.
    /// </summary>
    public string IdDocente { get; set; } = string.Empty;

    /// <summary>
    /// Identificador de la asignatura asignada.
    /// </summary>
    public string IdAsignatura { get; set; } = string.Empty;

    /// <summary>
    /// Día de la semana (1-6) donde se realiza la asignación.
    /// </summary>
    public int Dia { get; set; }

    /// <summary>
    /// Hora de inicio en formato HH:mm.
    /// </summary>
    public string HoraInicio { get; set; } = string.Empty;

    /// <summary>
    /// Hora de fin en formato HH:mm.
    /// </summary>
    public string HoraFin { get; set; } = string.Empty;

    /// <summary>
    /// Periodo académico al que pertenece la asignación (ej. 2026-1).
    /// </summary>
    public string Periodo { get; set; } = string.Empty;

    /// <summary>
    /// Estado de la asignación (ej. "Propuesta", "Confirmada").
    /// </summary>
    public string Estado { get; set; } = "Propuesta";

    /// <summary>
    /// Escenario de generación (ej. plan/jornada como "ING_DIURNA").
    /// </summary>
    public string Escenario { get; set; } = "ING_DIURNA";

    /// <summary>
    /// Navegación hacia la entidad Docente.
    /// </summary>
    public Docente? Docente { get; set; }

    /// <summary>
    /// Navegación hacia la entidad Asignatura.
    /// </summary>
    public Asignatura? Asignatura { get; set; }
}