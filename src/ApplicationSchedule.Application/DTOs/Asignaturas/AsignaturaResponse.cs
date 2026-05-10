namespace ApplicationSchedule.Application.DTOs.Asignaturas;

public class AsignaturaResponse
{
    public string IdAsignatura { get; set; } = string.Empty;

    public string IdPlan { get; set; } = string.Empty;

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public int Creditos { get; set; }

    public int Semestre { get; set; }

    public int MinEstudiantes { get; set; }

    public bool EsFijaTapsi { get; set; }
}