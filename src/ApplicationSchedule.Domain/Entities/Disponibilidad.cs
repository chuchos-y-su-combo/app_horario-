namespace ApplicationSchedule.Domain.Entities;

public class Disponibilidad
{
    public string IdDisponibilidad { get; set; } = Guid.NewGuid().ToString();

    public string IdDocente { get; set; } = string.Empty;

    public int DiaSemana { get; set; }

    public string HoraInicio { get; set; } = string.Empty;

    public string HoraFin { get; set; } = string.Empty;

    public Docente? Docente { get; set; }
}