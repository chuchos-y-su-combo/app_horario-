namespace ApplicationSchedule.Application.DTOs.Disponibilidades;

/// <summary>
/// DTO que resume la reducción de disponibilidad aplicada a un docente tras importar un currículum.
/// </summary>
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