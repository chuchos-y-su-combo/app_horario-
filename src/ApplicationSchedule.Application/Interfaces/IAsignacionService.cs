using ApplicationSchedule.Application.DTOs.Asignaciones;

namespace ApplicationSchedule.Application.Interfaces;

public interface IAsignacionService
{
    Task<List<AsignacionResponse>> ObtenerTodasAsync();

    Task<List<AsignacionResponse>> ObtenerPorDocenteAsync(string idDocente, string? periodo = null);

    Task<ResumenCargaDocenteResponse?> ObtenerResumenCargaDocenteAsync(string idDocente, string periodo);

    Task<AsignacionResponse> CrearAsync(CrearAsignacionRequest request);

    Task<bool> EliminarAsync(string idAsignacion);
}