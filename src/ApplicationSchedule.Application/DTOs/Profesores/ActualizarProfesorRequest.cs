namespace ApplicationSchedule.Application.DTOs.Profesores;

public class ActualizarProfesorRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Identificacion { get; set; } = string.Empty;
    public string TipoContrato { get; set; } = string.Empty;
}