using ApplicationSchedule.Application.DTOs.Horarios;

namespace ApplicationSchedule.Application.Interfaces;

public interface ICalendarioSemanalService
{
    /// <summary>
    /// Devuelve el horario en vista de calendario semanal (Lunes-Sábado),
    /// filtrable por plan de estudios y jornada.
    /// </summary>
    Task<CalendarioSemanalResponse> ObtenerCalendarioAsync(
        string semestre,
        string? idPlan = null,
        string? jornada = null,
        CancellationToken cancellationToken = default
    );
}