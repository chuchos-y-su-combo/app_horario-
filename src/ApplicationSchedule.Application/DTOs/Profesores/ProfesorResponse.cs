namespace ApplicationSchedule.Application.DTOs.Profesores;

public class ProfesorResponse
{
    public int IdProfesor { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Identificacion { get; set; } = string.Empty;
    public string TipoContrato { get; set; } = string.Empty;
}