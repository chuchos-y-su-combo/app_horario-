using ApplicationSchedule.Application.DTOs.Asignaturas;

namespace ApplicationSchedule.Application.Interfaces;

public interface IAsignaturaService
{
	Task<List<AsignaturaResponse>> ObtenerTodasAsync();

	Task<List<AsignaturaResponse>> ObtenerPorPlanEstudiosAsync(int idPlanEstudios);

	Task<AsignaturaResponse?> ObtenerPorIdAsync(int idAsignatura);

	Task<AsignaturaResponse> CrearAsync(CrearAsignaturaRequest request);

	Task<bool> ActualizarAsync(int idAsignatura, ActualizarAsignaturaRequest request);

	Task<bool> EliminarAsync(int idAsignatura);
}
