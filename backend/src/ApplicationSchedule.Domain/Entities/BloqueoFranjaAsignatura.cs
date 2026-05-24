namespace ApplicationSchedule.Domain.Entities;

/// <summary>
/// Representa una franja horaria bloqueada para una asignatura específica.
/// Si una asignatura tiene un bloqueo en un periodo, día y rango de horas,
/// el generador automático y los ajustes manuales no deben ubicarla allí.
/// </summary>
public class BloqueoFranjaAsignatura
{
    public string IdBloqueo { get; set; } = Guid.NewGuid().ToString();

    public string IdAsignatura { get; set; } = string.Empty;

    public string Periodo { get; set; } = string.Empty;

    public int Dia { get; set; }

    public string HoraInicio { get; set; } = string.Empty;

    public string HoraFin { get; set; } = string.Empty;

    public string? Motivo { get; set; }

    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;

    public Asignatura? Asignatura { get; set; }
}