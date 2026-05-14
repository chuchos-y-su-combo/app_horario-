namespace ApplicationSchedule.Application.DTOs.Horarios;

public class GenerarPropuestasHorarioResponse
{
    public string Periodo { get; set; } = string.Empty;

    public int TotalEscenariosProcesados { get; set; }

    public int TotalAsignaturasEvaluadas { get; set; }

    public int TotalPropuestasCreadas { get; set; }

    public int TotalAsignaturasNoAsignadas { get; set; }

    public List<ResumenEscenarioHorarioResponse> ResumenEscenarios { get; set; } = new();

    public List<PropuestaAsignacionResponse> Propuestas { get; set; } = new();

    public List<AsignaturaNoAsignadaResponse> NoAsignadas { get; set; } = new();

    public List<string> Mensajes { get; set; } = new();
}