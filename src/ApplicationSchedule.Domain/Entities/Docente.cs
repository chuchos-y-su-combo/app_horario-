namespace ApplicationSchedule.Domain.Entities;

/// <summary>
/// Entidad que representa un docente (profesor) con contrato, identificador y relaciones.
/// Contiene límites de carga y colecciones relacionadas como disponibilidades y asignaturas habilitadas.
/// </summary>
public class Docente
{
    /// <summary>
    /// Identificador único del docente.
    /// </summary>
    public string IdDocente { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Documento de identidad o número de identificación del docente.
    /// </summary>
    public string Identificacion { get; set; } = string.Empty;

    /// <summary>
    /// Nombre completo del docente.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// TC = Tiempo completo
    /// TP = Tiempo parcial
    /// </summary>
    public string TipoContrato { get; set; } = string.Empty;

    /// <summary>
    /// Número máximo de asignaturas que puede recibir según su contrato.
    /// </summary>
    public int MaxAsignaturas { get; set; }

    /// <summary>
    /// Asignaciones actuales del docente.
    /// </summary>
    public ICollection<Asignacion> Asignaciones { get; set; } = new List<Asignacion>();

    /// <summary>
    /// Asignaturas para las que el docente está habilitado.
    /// </summary>
    public ICollection<DocenteHabilitado> AsignaturasHabilitadas { get; set; } = new List<DocenteHabilitado>();

    /// <summary>
    /// Disponibilidades horarias del docente.
    /// </summary>
    public ICollection<Disponibilidad> Disponibilidades { get; set; } = new List<Disponibilidad>();
}