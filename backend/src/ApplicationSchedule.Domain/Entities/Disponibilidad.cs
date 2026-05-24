namespace ApplicationSchedule.Domain.Entities;

/// <summary>
/// Indica la disponibilidad horaria de un docente en un día específico.
/// Se utiliza para validar si un docente puede recibir una asignación en una franja.
/// </summary>
public class Disponibilidad
{
    /// <summary>
    /// Identificador único de la disponibilidad.
    /// </summary>
    public string IdDisponibilidad { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Identificador del docente al que pertenece la disponibilidad.
    /// </summary>
    public string IdDocente { get; set; } = string.Empty;

    /// <summary>
    /// Día de la semana (1-6) representado por entero.
    /// </summary>
    public int DiaSemana { get; set; }

    /// <summary>
    /// Hora de inicio en formato HH:mm.
    /// </summary>
    public string HoraInicio { get; set; } = string.Empty;

    /// <summary>
    /// Hora de fin en formato HH:mm.
    /// </summary>
    public string HoraFin { get; set; } = string.Empty;

    /// <summary>
    /// Relación de navegación hacia el docente.
    /// </summary>
    public Docente? Docente { get; set; }
}