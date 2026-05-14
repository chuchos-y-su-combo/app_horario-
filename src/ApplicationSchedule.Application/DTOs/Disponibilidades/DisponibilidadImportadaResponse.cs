namespace ApplicationSchedule.Application.DTOs.Disponibilidades;

public class DisponibilidadImportadaResponse
{
    public string NombreDocenteDetectado { get; set; } = string.Empty;

    public string? IdDocente { get; set; }

    public bool DocenteEncontrado { get; set; }

    public string TextoOriginal { get; set; } = string.Empty;

    public int RegistrosCreados { get; set; }

    public List<string> Mensajes { get; set; } = new();

    public List<DisponibilidadDocenteResponse> Disponibilidades { get; set; } = new();
}