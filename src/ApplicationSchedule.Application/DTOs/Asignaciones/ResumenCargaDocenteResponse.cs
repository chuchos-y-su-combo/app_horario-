namespace ApplicationSchedule.Application.DTOs.Asignaciones;

public class ResumenCargaDocenteResponse
{
    public string IdDocente { get; set; } = string.Empty;

    public string NombreDocente { get; set; } = string.Empty;

    public string TipoContrato { get; set; } = string.Empty;

    public int MaxAsignaturas { get; set; }

    public int AsignaturasActuales { get; set; }

    public bool PuedeAsignarMas { get; set; }

    public string Periodo { get; set; } = string.Empty;
}