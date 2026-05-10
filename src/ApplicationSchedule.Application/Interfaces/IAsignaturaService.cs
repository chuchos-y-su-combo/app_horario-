using ApplicationSchedule.Application.DTOs.Asignaturas;

namespace ApplicationSchedule.Application.Interfaces;

public interface IAsignaturaService
{
    Task<List<AsignaturaResponse>> ObtenerTodasAsync();

    Task<List<AsignaturaResponse>> ObtenerPorPlanAsync(string idPlan);

    Task<AsignaturaResponse?> ObtenerPorIdAsync(string idAsignatura);

    Task<List<AsignaturaResponse>> ObtenerFijasTapsiAsync();

    Task<AsignaturaResponse> CrearAsync(CrearAsignaturaRequest request);

    Task<bool> ActualizarAsync(string idAsignatura, ActualizarAsignaturaRequest request);

    Task<int> MarcarObligatoriasTapsiComoFijasAsync();

    Task<bool> EliminarAsync(string idAsignatura);
}