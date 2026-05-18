using ApplicationSchedule.Application.DTOs.Asignaciones;

namespace ApplicationSchedule.Application.Interfaces;

public interface IAsignacionService
{
    Task<List<AsignacionResponse>> ObtenerTodasAsync();

    Task<List<string>> ObtenerPeriodosHistoricosAsync();

    Task<List<AsignacionResponse>> ObtenerFiltradasAsync(int? semestre, string? idDocente, string? idAsignatura, string? periodo, string? estado = null);

    Task<List<AsignacionResponse>> ObtenerPorDocenteAsync(string idDocente, string? periodo = null);

    Task<List<AsignacionResponse>> ObtenerPropuestasPorPeriodoAsync(string periodo);

    Task<ResumenCargaDocenteResponse?> ObtenerResumenCargaDocenteAsync(string idDocente, string periodo);

    Task<AsignacionResponse> CrearAsync(CrearAsignacionRequest request);

    Task<bool> EliminarAsync(string idAsignacion);


    Task<AsignacionResponse> AsignarManualmenteAsync(AsignarAsignaturaManualRequest request);

    Task<List<AsignaturaDisponibleParaDocenteResponse>> ObtenerAsignaturasDisponiblesParaDocenteAsync(
        string idDocente,
        string periodo
    );

    Task<AsignacionResponse> AjustarAsync(string idAsignacion, AjustarAsignacionRequest request);

    Task<ResultadoConfirmacionResponse> ConfirmarAsync(ConfirmarAsignacionesRequest request);

    Task<AsignacionResponse> CancelarAsync(string idAsignacion);

    Task<AsignacionResponse> AsignarDiaAsync(string idAsignacion, AsignarDiaRequest request);
}