namespace ApplicationSchedule.Application.DTOs.Horarios;

/// <summary>
/// DTO que describe una asignatura que no pudo ser asignada en la generación de propuestas.
/// Incluye razón y metadatos para diagnóstico.
/// </summary>
public class AsignaturaNoAsignadaResponse
{
    public string Escenario { get; set; } = string.Empty;

    public string IdPlan { get; set; } = string.Empty;

    public string NombrePlan { get; set; } = string.Empty;

    public string IdAsignatura { get; set; } = string.Empty;

    public string CodigoAsignatura { get; set; } = string.Empty;

    public string NombreAsignatura { get; set; } = string.Empty;

    public string Motivo { get; set; } = string.Empty;
}