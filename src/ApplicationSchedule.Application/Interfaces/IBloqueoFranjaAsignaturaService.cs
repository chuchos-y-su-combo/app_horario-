using ApplicationSchedule.Application.DTOs.Bloqueos;

namespace ApplicationSchedule.Application.Interfaces;

public interface IBloqueoFranjaAsignaturaService
{
    Task<List<BloqueoFranjaAsignaturaResponse>> ObtenerTodosAsync(
        string? periodo = null,
        CancellationToken cancellationToken = default
    );

    Task<List<BloqueoFranjaAsignaturaResponse>> ObtenerPorAsignaturaAsync(
        string idAsignatura,
        string? periodo = null,
        CancellationToken cancellationToken = default
    );

    Task<BloqueoFranjaAsignaturaResponse> CrearAsync(
        string idAsignatura,
        CrearBloqueoFranjaAsignaturaRequest request,
        CancellationToken cancellationToken = default
    );

    Task<bool> EliminarAsync(
        string idBloqueo,
        CancellationToken cancellationToken = default
    );
}