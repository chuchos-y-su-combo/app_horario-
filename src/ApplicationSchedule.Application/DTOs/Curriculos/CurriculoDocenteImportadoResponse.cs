namespace ApplicationSchedule.Application.DTOs.Curriculos;

/// <summary>
/// Resume el resultado de importación para una hoja/docente.
/// </summary>
public class CurriculoDocenteImportadoResponse
{
    public string Hoja { get; set; } = string.Empty;

    public string NombreDocenteDetectado { get; set; } = string.Empty;

    public string? IdDocente { get; set; }

    public bool DocenteEncontrado { get; set; }

    public int TotalAsignaturasDetectadas { get; set; }

    public int TotalAsignaturasHabilitadas { get; set; }

    public List<CurriculoAsignaturaDetectadaResponse> Asignaturas { get; set; } = new();
}