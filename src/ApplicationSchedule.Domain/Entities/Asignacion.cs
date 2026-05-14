namespace ApplicationSchedule.Domain.Entities;

public class Asignacion
{
    public string IdAsignacion { get; set; } = Guid.NewGuid().ToString();

    public string IdDocente { get; set; } = string.Empty;

    public string IdAsignatura { get; set; } = string.Empty;

    public int Dia { get; set; }

    public string HoraInicio { get; set; } = string.Empty;

    public string HoraFin { get; set; } = string.Empty;

    public string Periodo { get; set; } = string.Empty;

    public string Estado { get; set; } = "Propuesta";
    public string Escenario { get; set; } = "ING_DIURNA";

    public Docente? Docente { get; set; }

    public Asignatura? Asignatura { get; set; }
}