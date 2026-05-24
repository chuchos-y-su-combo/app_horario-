namespace ApplicationSchedule.Application.DTOs.Curriculos;

/// <summary>
/// Petición para habilitar manualmente una asignatura a un docente.
/// </summary>
public class HabilitarAsignaturaRequest
{
    public string IdAsignatura { get; set; } = string.Empty;
}
