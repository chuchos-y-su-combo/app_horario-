using ApplicationSchedule.Application.DTOs.Disponibilidades;
using ApplicationSchedule.Application.DTOs.Profesores;

namespace ApplicationSchedule.Application.Interfaces;

/// <summary>
/// Operaciones para gestionar la entidad Profesor desde la capa de aplicación.
/// </summary>
public interface IProfesorService
{
    /// <summary>
    /// Recupera todos los profesores.
    /// </summary>
    Task<List<ProfesorResponse>> ObtenerTodosAsync();

    /// <summary>
    /// Obtiene un profesor por su identificador.
    /// </summary>
    Task<ProfesorResponse?> ObtenerPorIdAsync(string idProfesor);

    /// <summary>
    /// Crea un nuevo profesor.
    /// </summary>
    Task<ProfesorResponse> CrearAsync(CrearProfesorRequest request);

    /// <summary>
    /// Actualiza un profesor existente.
    /// </summary>
    Task<bool> ActualizarAsync(string idProfesor, ActualizarProfesorRequest request);

    /// <summary>
    /// Elimina un profesor por su identificador.
    /// </summary>
    Task<bool> EliminarAsync(string idProfesor);

    /// <summary>
    /// Retorna las franjas de disponibilidad horaria de un docente.
    /// </summary>
    Task<List<DisponibilidadDocenteResponse>> ObtenerDisponibilidadAsync(string idProfesor);

    /// <summary>
    /// Elimina TODOS los docentes y sus asignaciones asociadas.
    /// </summary>
    Task<int> EliminarTodosAsync();
}