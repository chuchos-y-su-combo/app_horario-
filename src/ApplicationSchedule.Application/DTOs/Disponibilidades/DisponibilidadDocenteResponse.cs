namespace ApplicationSchedule.Application.DTOs.Disponibilidades;

public class DisponibilidadDocenteResponse
{
    public string IdDisponibilidad { get; set; } = string.Empty;

    public string IdDocente { get; set; } = string.Empty;

    public int DiaSemana { get; set; }

    public string DiaNombre { get; set; } = string.Empty;

    public string HoraInicio { get; set; } = string.Empty;

    public string HoraFin { get; set; } = string.Empty;
}