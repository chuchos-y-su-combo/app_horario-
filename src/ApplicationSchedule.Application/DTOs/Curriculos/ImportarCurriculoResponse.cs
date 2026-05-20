using ApplicationSchedule.Application.DTOs.Disponibilidades;

namespace ApplicationSchedule.Application.DTOs.Curriculos;

/// <summary>
/// Resultado general de la carga de currículo desde Excel.
/// </summary>
public class ImportarCurriculoResponse
{
    public string Archivo { get; set; } = string.Empty;

    public int TotalHojasProcesadas { get; set; }

    public int TotalDocentesEncontrados { get; set; }

    public int TotalRelacionesCreadas { get; set; }

    public int TotalRelacionesExistentes { get; set; }

    public int TotalAsignaturasNoEncontradas { get; set; }

    public int TotalDisponibilidadesCreadas { get; set; }

    public List<string> DocentesNoEncontrados { get; set; } = new();

    public List<CurriculoDocenteImportadoResponse> Detalle { get; set; } = new();

    public List<DisponibilidadImportadaResponse> Disponibilidades { get; set; } = new();
}