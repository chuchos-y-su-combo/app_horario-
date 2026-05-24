using ApplicationSchedule.Application.DTOs.Asignaciones;

namespace ApplicationSchedule.Application.Interfaces;

/// <summary>
/// Contrato para la gestión de asignaciones (creación, ajuste, confirmación y consultas).
/// </summary>
public interface IAsignacionService
{
    /// <summary>
    /// Recupera todas las asignaciones disponibles.
    /// </summary>
    Task<List<AsignacionResponse>> ObtenerTodasAsync();

    /// <summary>
    /// Obtiene los periodos históricos registrados en las asignaciones.
    /// </summary>
    Task<List<string>> ObtenerPeriodosHistoricosAsync();

    /// <summary>
    /// Recupera asignaciones aplicando filtros opcionales.
    /// </summary>
    Task<List<AsignacionResponse>> ObtenerFiltradasAsync(int? semestre, string? idDocente, string? idAsignatura, string? periodo, string? estado = null);

    /// <summary>
    /// Obtiene asignaciones de un docente, opcionalmente por periodo.
    /// </summary>
    Task<List<AsignacionResponse>> ObtenerPorDocenteAsync(string idDocente, string? periodo = null);

    /// <summary>
    /// Recupera las propuestas de asignación para un periodo.
    /// </summary>
    Task<List<AsignacionResponse>> ObtenerPropuestasPorPeriodoAsync(string periodo);

    /// <summary>
    /// Genera un resumen de la carga docente para un periodo.
    /// </summary>
    Task<ResumenCargaDocenteResponse?> ObtenerResumenCargaDocenteAsync(string idDocente, string periodo);

    /// <summary>
    /// Crea una nueva asignación aplicando validaciones de negocio.
    /// </summary>
    Task<AsignacionResponse> CrearAsync(CrearAsignacionRequest request);

    /// <summary>
    /// Elimina una asignación por su identificador.
    /// </summary>
    Task<bool> EliminarAsync(string idAsignacion);

    /// <summary>
    /// Asigna manualmente una asignatura a un docente (operación administrativa).
    /// </summary>
    Task<AsignacionResponse> AsignarManualmenteAsync(AsignarAsignaturaManualRequest request);

    /// <summary>
    /// Obtiene asignaturas disponibles para un docente en un periodo.
    /// </summary>
    Task<List<AsignaturaDisponibleParaDocenteResponse>> ObtenerAsignaturasDisponiblesParaDocenteAsync(
        string idDocente,
        string periodo
    );

    /// <summary>
    /// Ajusta los datos de una asignación existente.
    /// </summary>
    Task<AsignacionResponse> AjustarAsync(string idAsignacion, AjustarAsignacionRequest request);

    /// <summary>
    /// Confirma un conjunto de asignaciones (p. ej. pasar de propuesta a confirmado).
    /// </summary>
    Task<ResultadoConfirmacionResponse> ConfirmarAsync(ConfirmarAsignacionesRequest request);

    /// <summary>
    /// Cancela una asignación existente.
    /// </summary>
    Task<AsignacionResponse> CancelarAsync(string idAsignacion);

    /// <summary>
    /// Reubica una asignación a otro día/modo mediante petición de asignación diaria.
    /// </summary>
    Task<AsignacionResponse> AsignarDiaAsync(string idAsignacion, AsignarDiaRequest request);
}