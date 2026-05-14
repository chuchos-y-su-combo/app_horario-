namespace ApplicationSchedule.Application.DTOs.Horarios;

public class PropuestaAsignacionResponse
{
    public string IdAsignacion { get; set; } = string.Empty;

    public string Escenario { get; set; } = string.Empty;

    public string IdPlan { get; set; } = string.Empty;

    public string NombrePlan { get; set; } = string.Empty;

    public string IdAsignatura { get; set; } = string.Empty;

    public string CodigoAsignatura { get; set; } = string.Empty;

    public string NombreAsignatura { get; set; } = string.Empty;

    public string IdDocente { get; set; } = string.Empty;

    public string NombreDocente { get; set; } = string.Empty;

    public int Dia { get; set; }

    public string DiaNombre { get; set; } = string.Empty;

    public string HoraInicio { get; set; } = string.Empty;

    public string HoraFin { get; set; } = string.Empty;

    public string Estado { get; set; } = "Propuesta";
}