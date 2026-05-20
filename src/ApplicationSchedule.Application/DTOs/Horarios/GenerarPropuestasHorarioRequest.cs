namespace ApplicationSchedule.Application.DTOs.Horarios;

/// <summary>
/// DTO para solicitar la generación de propuestas de horario. Contiene parámetros
/// como periodo, escenarios y restricciones a aplicar.
/// </summary>
public class GenerarPropuestasHorarioRequest
{
    public string Periodo { get; set; } = string.Empty;

    public List<string> Escenarios { get; set; } = new();

    public int SemestreIngenieria { get; set; } = 1;

    public bool BorrarPropuestasPrevias { get; set; } = true;
}