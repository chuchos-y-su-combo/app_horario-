namespace ApplicationSchedule.Application.DTOs.Horarios;

public class GenerarPropuestasHorarioRequest
{
    public string Periodo { get; set; } = string.Empty;

    public List<string> Escenarios { get; set; } = new();

    public int SemestreIngenieria { get; set; } = 1;

    public bool BorrarPropuestasPrevias { get; set; } = true;
}