namespace ApplicationSchedule.Application.DTOs.Curriculos;

/// <summary>
/// Representa una asignatura encontrada en el Excel durante la importación.
/// </summary>
public class CurriculoAsignaturaDetectadaResponse
{
    public string Hoja { get; set; } = string.Empty;

    public int Fila { get; set; }

    public string? CodigoDetectado { get; set; }

    public string NombreDetectado { get; set; } = string.Empty;

    public string? IdAsignatura { get; set; }

    public string? NombreAsignaturaBaseDatos { get; set; }

    public bool FueHabilitada { get; set; }

    public string Estado { get; set; } = string.Empty;

    public string Mensaje { get; set; } = string.Empty;
}