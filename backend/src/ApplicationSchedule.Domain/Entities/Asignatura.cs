namespace ApplicationSchedule.Domain.Entities;

/// <summary>
/// Entidad que representa una asignatura del plan de estudios.
/// Contiene su relación con el plan, la carga académica y banderas de TAPSI.
/// </summary>
public class Asignatura
{
    /// <summary>
    /// Identificador único de la asignatura.
    /// </summary>
    public string IdAsignatura { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Identificador del plan de estudios al que pertenece.
    /// </summary>
    public string IdPlan { get; set; } = string.Empty;

    /// <summary>
    /// Código único de la asignatura.
    /// </summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nombre descriptivo de la asignatura.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Número de créditos de la asignatura.
    /// </summary>
    public int Creditos { get; set; }

    /// <summary>
    /// Semestre en el que se dicta la asignatura.
    /// </summary>
    public int Semestre { get; set; }

    /// <summary>
    /// Cantidad mínima de estudiantes requerida para abrir el grupo.
    /// </summary>
    public int MinEstudiantes { get; set; } = 15;

    /// <summary>
    /// Indica si la asignatura es fija en TAPSI.
    /// </summary>
    public bool EsFijaTapsi { get; set; }

    /// <summary>
    /// Indica si la asignatura es opcional para TAPSI diurna.
    /// </summary>
    public bool EsOpcionalTapsiDiurna { get; set; }

    /// <summary>
    /// Indica si la asignatura pertenece al área de formación profesional.
    /// En semestre 1 se generan todas; en semestres 2+ solo las marcadas con true.
    /// </summary>
    public bool EsAreaProfesional { get; set; }

    /// <summary>
    /// Aula asignada a la asignatura (ej. AULA-F301).
    /// </summary>
    public string Aula { get; set; } = string.Empty;

    /// <summary>
    /// Plan de estudios asociado a la asignatura.
    /// </summary>
    public PlanEstudio? PlanEstudio { get; set; }

    /// <summary>
    /// Colección de asignaciones generadas para esta asignatura.
    /// </summary>
    public ICollection<Asignacion> Asignaciones { get; set; } = new List<Asignacion>();

    /// <summary>
    /// Docentes habilitados para dictar esta asignatura.
    /// </summary>
    public ICollection<DocenteHabilitado> DocentesHabilitados { get; set; } = new List<DocenteHabilitado>();

    /// <summary>
    /// Bloqueos de franja configurados para esta asignatura.
    /// </summary>
    public ICollection<BloqueoFranjaAsignatura> BloqueosFranja { get; set; } = new List<BloqueoFranjaAsignatura>();
}
