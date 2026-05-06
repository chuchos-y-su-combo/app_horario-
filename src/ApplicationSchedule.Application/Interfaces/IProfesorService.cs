using ApplicationSchedule.Application.DTOs.Profesores;

namespace ApplicationSchedule.Application.Interfaces;

public interface IProfesorService
{
    Task<List<ProfesorResponse>> ObtenerTodosAsync();
    Task<ProfesorResponse?> ObtenerPorIdAsync(int idProfesor);
    Task<ProfesorResponse> CrearAsync(CrearProfesorRequest request);
    Task<bool> ActualizarAsync(int idProfesor, ActualizarProfesorRequest request);
    Task<bool> EliminarAsync(int idProfesor);
}