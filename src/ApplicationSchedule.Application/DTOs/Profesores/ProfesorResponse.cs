namespace ApplicationSchedule.Application.DTOs.Profesores;

public class ProfesorResponse
{
    public string IdProfesor { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Identificacion { get; set; } = string.Empty;

    public string TipoContrato { get; set; } = string.Empty;

    public int MaxAsignaturas { get; set; }
}