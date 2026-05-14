namespace ApplicationSchedule.Domain.Entities;

public class Docente
{
    public string IdDocente { get; set; } = Guid.NewGuid().ToString();

    public string Identificacion { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// TC = Tiempo completo
    /// TP = Tiempo parcial
    /// </summary>
    public string TipoContrato { get; set; } = string.Empty;

    public int MaxAsignaturas { get; set; }

    public ICollection<Asignacion> Asignaciones { get; set; } = new List<Asignacion>();

    public ICollection<DocenteHabilitado> AsignaturasHabilitadas { get; set; } = new List<DocenteHabilitado>();
    public ICollection<Disponibilidad> Disponibilidades { get; set; } = new List<Disponibilidad>();
}