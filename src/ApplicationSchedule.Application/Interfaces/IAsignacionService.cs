using ApplicationSchedule.Application.DTOs.Asignaciones;

namespace ApplicationSchedule.Application.Interfaces;

public interface IAsignacionService
{
    Task<List<AsignacionResponse>> ObtenerTodasAsync();

    Task<List<AsignacionResponse>> ObtenerPorDocenteAsync(string idDocente, string? periodo = null);

    Task<List<AsignacionResponse>> ObtenerPropuestasPorPeriodoAsync(string periodo);

    Task<ResumenCargaDocenteResponse?> ObtenerResumenCargaDocenteAsync(string idDocente, string periodo);

    Task<AsignacionResponse> CrearAsync(CrearAsignacionRequest request);

    Task<bool> EliminarAsync(string idAsignacion);

    // ── Issue #10 ──────────────────────────────────────────────────────────

    Task<AsignacionResponse> AsignarManualmenteAsync(AsignarAsignaturaManualRequest request);

    Task<List<AsignaturaDisponibleParaDocenteResponse>> ObtenerAsignaturasDisponiblesParaDocenteAsync(
        string idDocente,
        string periodo
    );

    // ── Issue #12 ──────────────────────────────────────────────────────────

    Task<AsignacionResponse> AjustarAsync(string idAsignacion, AjustarAsignacionRequest request);

    Task<ResultadoConfirmacionResponse> ConfirmarAsync(ConfirmarAsignacionesRequest request);

    Task<AsignacionResponse> CancelarAsync(string idAsignacion);
}