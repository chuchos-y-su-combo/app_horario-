using ApplicationSchedule.Application.DTOs.Asignaturas;

namespace ApplicationSchedule.Application.DTOs.Tapsi;

public class TapsiDiurnaPlanResponse
{
    public string Jornada { get; set; } = "Diurna";

    public int TopeCreditosDiurna { get; set; } = 18;

    public int TopeCreditosJornadaExtendida { get; set; } = 15;

    public int CreditosFijosTapsi { get; set; }

    public int CreditosAdicionalesRequeridos { get; set; }

    public int CreditosTotalesRequeridosDiurna { get; set; }

    public string Regla { get; set; } = string.Empty;

    public List<AsignaturaResponse> AsignaturasFijas { get; set; } = new();

    public List<AsignaturaResponse> OpcionesAdicionalesDiurna { get; set; } = new();
}