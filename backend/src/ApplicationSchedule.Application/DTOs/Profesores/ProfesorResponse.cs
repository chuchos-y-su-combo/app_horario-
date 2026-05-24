namespace ApplicationSchedule.Application.DTOs.Profesores;

/// <summary>
/// DTO de salida que representa la información pública de un docente/profesor.
/// </summary>
public class ProfesorResponse
{
    public string IdProfesor { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Identificacion { get; set; } = string.Empty;

    public string TipoContrato { get; set; } = string.Empty;

    public int MaxAsignaturas { get; set; }
}