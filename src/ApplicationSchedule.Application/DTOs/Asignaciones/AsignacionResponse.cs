namespace ApplicationSchedule.Application.DTOs.Asignaciones;

public class AsignacionResponse
{
    public string IdAsignacion { get; set; } = string.Empty;

    public string IdDocente { get; set; } = string.Empty;

    public string NombreDocente { get; set; } = string.Empty;

    public string TipoContrato { get; set; } = string.Empty;

    public int MaxAsignaturas { get; set; }

    public int AsignaturasActuales { get; set; }

    public string IdAsignatura { get; set; } = string.Empty;

    public string CodigoAsignatura { get; set; } = string.Empty;

    public string NombreAsignatura { get; set; } = string.Empty;

    public int Dia { get; set; }

    public string HoraInicio { get; set; } = string.Empty;

    public string HoraFin { get; set; } = string.Empty;

    public string Periodo { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;
}