using ApplicationSchedule.Application.DTOs.Horarios;

namespace ApplicationSchedule.Application.Interfaces;

public interface ICalendarioSemanalService
{
    /// <summary>
    /// Issue #39: Devuelve el horario en vista de calendario semanal (Lunes-Sábado),
    /// filtrable por plan de estudios y jornada.
    /// </summary>
    Task<CalendarioSemanalResponse> ObtenerCalendarioAsync(
        string semestre,
        string? idPlan = null,
        string? jornada = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Issue #40: Devuelve el horario individual de un docente en vista de
    /// calendario semanal (Lunes-Sábado), filtrable opcionalmente por semestre.
    /// </summary>
    Task<CalendarioDocenteResponse> ObtenerCalendarioDocenteAsync(
        string idDocente,
        string semestre,
        CancellationToken cancellationToken = default
    );
}