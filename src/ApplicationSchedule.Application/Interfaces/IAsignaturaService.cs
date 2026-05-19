using ApplicationSchedule.Application.DTOs.Asignaturas;
using ApplicationSchedule.Application.DTOs.Tapsi;

namespace ApplicationSchedule.Application.Interfaces;

/// <summary>
/// Operaciones relacionadas con asignaturas: consulta, creación y marcado de opciones TAPSI.
/// </summary>
public interface IAsignaturaService
{
    /// <summary>
    /// Obtiene las asignaturas opcionales del plan TAPSI en jornada diurna.
    /// </summary>
    Task<List<AsignaturaResponse>> ObtenerOpcionalesTapsiDiurnaAsync();

    /// <summary>
    /// Obtiene el plan TAPSI diurno con información resumida por semestre.
    /// </summary>
    Task<TapsiDiurnaPlanResponse> ObtenerPlanTapsiDiurnaAsync();

    /// <summary>
    /// Marca las asignaturas opcionales TAPSI como tales en la base de datos.
    /// </summary>
    Task<int> MarcarOpcionalesTapsiDiurnaAsync();

    /// <summary>
    /// Recupera todas las asignaturas registradas.
    /// </summary>
    Task<List<AsignaturaResponse>> ObtenerTodasAsync();

    /// <summary>
    /// Obtiene asignaturas pertenecientes a un plan de estudios.
    /// </summary>
    Task<List<AsignaturaResponse>> ObtenerPorPlanAsync(string idPlan);

    /// <summary>
    /// Consulta una asignatura por su identificador.
    /// </summary>
    Task<AsignaturaResponse?> ObtenerPorIdAsync(string idAsignatura);

    /// <summary>
    /// Obtiene las asignaturas fijas para TAPSI.
    /// </summary>
    Task<List<AsignaturaResponse>> ObtenerFijasTapsiAsync();

    /// <summary>
    /// Crea una nueva asignatura.
    /// </summary>
    Task<AsignaturaResponse> CrearAsync(CrearAsignaturaRequest request);

    /// <summary>
    /// Actualiza una asignatura existente.
    /// </summary>
    Task<bool> ActualizarAsync(string idAsignatura, ActualizarAsignaturaRequest request);

    /// <summary>
    /// Marca asignaturas obligatorias de TAPSI como fijas.
    /// </summary>
    Task<int> MarcarObligatoriasTapsiComoFijasAsync();

    /// <summary>
    /// Elimina una asignatura por su identificador.
    /// </summary>
    Task<bool> EliminarAsync(string idAsignatura);
}