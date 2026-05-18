namespace ApplicationSchedule.Application.DTOs.Bloqueos;

public class BloqueoFranjaAsignaturaResponse
{
    public string IdBloqueo { get; set; } = string.Empty;

    public string IdAsignatura { get; set; } = string.Empty;

    public string CodigoAsignatura { get; set; } = string.Empty;

    public string NombreAsignatura { get; set; } = string.Empty;

    public string Periodo { get; set; } = string.Empty;

    public int Dia { get; set; }

    public string DiaNombre { get; set; } = string.Empty;

    public string HoraInicio { get; set; } = string.Empty;

    public string HoraFin { get; set; } = string.Empty;

    public string? Motivo { get; set; }

    public DateTime FechaCreacionUtc { get; set; }
}