using ApplicationSchedule.Application.DTOs.Profesores;

namespace ApplicationSchedule.Application.Interfaces;

public interface IProfesorService
{
    Task<List<ProfesorResponse>> ObtenerTodosAsync();

    Task<ProfesorResponse?> ObtenerPorIdAsync(string idProfesor);

    Task<ProfesorResponse> CrearAsync(CrearProfesorRequest request);

    Task<bool> ActualizarAsync(string idProfesor, ActualizarProfesorRequest request);

    Task<bool> EliminarAsync(string idProfesor);
}