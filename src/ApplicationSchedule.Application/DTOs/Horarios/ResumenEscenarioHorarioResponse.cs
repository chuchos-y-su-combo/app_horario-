namespace ApplicationSchedule.Application.DTOs.Horarios;

public class ResumenEscenarioHorarioResponse
{
    public string Escenario { get; set; } = string.Empty;

    public string IdPlan { get; set; } = string.Empty;

    public string NombrePlan { get; set; } = string.Empty;

    public int TotalAsignaturasEvaluadas { get; set; }

    public int TotalPropuestasCreadas { get; set; }

    public int TotalNoAsignadas { get; set; }
}