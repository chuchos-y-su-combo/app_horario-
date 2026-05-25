using ApplicationSchedule.Application.DTOs.Bloqueos;

namespace ApplicationSchedule.Application.Interfaces;

/// <summary>
/// Operaciones para gestionar bloqueos de franjas horarios por asignatura.
/// </summary>
public interface IBloqueoFranjaAsignaturaService
{
    /// <summary>
    /// Recupera todos los bloqueos, opcionalmente filtrados por periodo.
    /// </summary>
    Task<List<BloqueoFranjaAsignaturaResponse>> ObtenerTodosAsync(
        string? periodo = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Obtiene los bloqueos de una asignatura específica.
    /// </summary>
    Task<List<BloqueoFranjaAsignaturaResponse>> ObtenerPorAsignaturaAsync(
        string idAsignatura,
        string? periodo = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Crea un bloqueo de franja para una asignatura.
    /// </summary>
    Task<BloqueoFranjaAsignaturaResponse> CrearAsync(
        string idAsignatura,
        CrearBloqueoFranjaAsignaturaRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Crea un bloqueo global de franja: aplica a TODAS las asignaturas del período.
    /// Devuelve el número de registros creados.
    /// </summary>
    Task<int> CrearGlobalAsync(
        CrearBloqueoFranjaAsignaturaRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Elimina un bloqueo por su identificador.
    /// </summary>
    Task<bool> EliminarAsync(
        string idBloqueo,
        CancellationToken cancellationToken = default
    );
}