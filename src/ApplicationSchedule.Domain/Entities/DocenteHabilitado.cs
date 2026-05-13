namespace ApplicationSchedule.Domain.Entities;

/// <summary>
/// Representa una asignatura que un docente está habilitado para dictar.
/// Esta información puede venir del currículo cargado desde Excel.
/// </summary>
public class DocenteHabilitado
{
    public string IdDocente { get; set; } = string.Empty;

    public string IdAsignatura { get; set; } = string.Empty;

    public DateTime FechaHabilitacion { get; set; } = DateTime.UtcNow;

    public string Fuente { get; set; } = "Excel";

    public Docente? Docente { get; set; }

    public Asignatura? Asignatura { get; set; }
}