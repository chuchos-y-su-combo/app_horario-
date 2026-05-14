using ApplicationSchedule.Application.DTOs.Horarios;

namespace ApplicationSchedule.Application.Interfaces;

public interface IGeneradorHorarioService
{
    Task<GenerarPropuestasHorarioResponse> GenerarPropuestasAsync(
        GenerarPropuestasHorarioRequest request,
        CancellationToken cancellationToken = default
    );
}