namespace ApplicationSchedule.Domain.Entities;

public class Asignatura
{
    public string IdAsignatura { get; set; } = Guid.NewGuid().ToString();

    public string IdPlan { get; set; } = string.Empty;

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public int Creditos { get; set; }

    public int Semestre { get; set; }

    public int MinEstudiantes { get; set; } = 15;

    public bool EsFijaTapsi { get; set; }

    public PlanEstudio? PlanEstudio { get; set; }

    public ICollection<Asignacion> Asignaciones { get; set; } = new List<Asignacion>();
}