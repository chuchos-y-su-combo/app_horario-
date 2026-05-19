namespace ApplicationSchedule.Application.DTOs.Asignaciones;

/// <summary>
/// DTO que indica que una asignatura está disponible para ser dictada por un docente.
/// Incluye detalles como compatibilidad de horario y requisitos.
/// </summary>
public class AsignaturaDisponibleParaDocenteResponse
{
    public string IdAsignatura { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Creditos { get; set; }
    public int Semestre { get; set; }

    /// <summary>
    /// True si el docente tiene esta asignatura en su currículo habilitado.
    /// </summary>
    public bool HabilitadaPorCurriculo { get; set; }

    /// <summary>
    /// True si la asignatura ya fue asignada al docente en el periodo consultado.
    /// </summary>
    public bool YaAsignadaEnPeriodo { get; set; }
}