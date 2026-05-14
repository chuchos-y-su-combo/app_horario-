namespace ApplicationSchedule.Application.DTOs.Disponibilidades;

public class ReduccionDisponibilidadResponse
{
    public string IdDocente { get; set; } = string.Empty;

    public string NombreDocente { get; set; } = string.Empty;

    public string NombreAsignatura { get; set; } = string.Empty;

    public string Periodo { get; set; } = string.Empty;

    public int BloquesEliminados { get; set; }

    public List<string> BloquesAfectados { get; set; } = new();

    public string Mensaje { get; set; } = string.Empty;
}