namespace ApplicationSchedule.Domain.Entities;

public class PlanEstudio
{
    public string IdPlan { get; set; } = Guid.NewGuid().ToString();

    public string NombrePlan { get; set; } = string.Empty;

    public string Jornada { get; set; } = string.Empty;

    public ICollection<Asignatura> Asignaturas { get; set; } = new List<Asignatura>();
}