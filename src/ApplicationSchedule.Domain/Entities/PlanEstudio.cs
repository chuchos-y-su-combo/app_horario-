namespace ApplicationSchedule.Domain.Entities;

/// <summary>
/// Representa un plan de estudios (carrera/jornada) que agrupa asignaturas.
/// </summary>
public class PlanEstudio
{
    /// <summary>
    /// Identificador del plan.
    /// </summary>
    public string IdPlan { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Nombre descriptivo del plan de estudios.
    /// </summary>
    public string NombrePlan { get; set; } = string.Empty;

    /// <summary>
    /// Jornada del plan (ej. diurna, nocturna).
    /// </summary>
    public string Jornada { get; set; } = string.Empty;

    /// <summary>
    /// Colección de asignaturas que pertenecen al plan.
    /// </summary>
    public ICollection<Asignatura> Asignaturas { get; set; } = new List<Asignatura>();
}