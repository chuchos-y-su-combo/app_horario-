namespace ApplicationSchedule.Domain.Entities;

/// <summary>
/// Entidad que representa una asignatura del plan de estudios.
/// Contiene información básica como créditos, plan y flags específicos de TAPSI.
/// </summary>
public class Asignatura
{
    /// <summary>
    /// Identificador único de la asignatura.
    /// </summary>
    public string IdAsignatura { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Nombre descriptivo de la asignatura.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Créditos (horas) de la asignatura.
    /// </summary>
    public int Creditos { get; set; }

    /// <summary>
    /// Identificador del plan de estudios al que pertenece.
    /// </summary>
    public string IdPlan { get; set; } = string.Empty;

    /// <summary>
    /// Indica si la asignatura es considerada fija en TAPSI.
    /// </summary>
    public bool EsFijaTapsi { get; set; }

    /// <summary>
    /// Indica si la asignatura es obligatoria en TAPSI.
    /// </summary>
    public bool EsObligatoriaTapsi { get; set; }

    /// <summary>
    /// Colección de asignaciones asociadas a esta asignatura.
    /// </summary>
    public ICollection<Asignacion> Asignaciones { get; set; } = new List<Asignacion>();
}
    public bool EsFijaTapsi { get; set; }
    public bool EsOpcionalTapsiDiurna { get; set; }

    public PlanEstudio? PlanEstudio { get; set; }

    public ICollection<Asignacion> Asignaciones { get; set; } = new List<Asignacion>();

    public ICollection<DocenteHabilitado> DocentesHabilitados { get; set; } = new List<DocenteHabilitado>();
    public ICollection<BloqueoFranjaAsignatura> BloqueosFranja { get; set; } = new List<BloqueoFranjaAsignatura>();
}