namespace ApplicationSchedule.Application.DTOs.Curriculos;

/// <summary>
/// Representa una asignatura que un docente puede dictar.
/// </summary>
public class AsignaturaHabilitadaDocenteResponse
{
    public string IdAsignatura { get; set; } = string.Empty;

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public int Creditos { get; set; }

    public int Semestre { get; set; }

    public string Fuente { get; set; } = string.Empty;

    public DateTime FechaHabilitacion { get; set; }
}