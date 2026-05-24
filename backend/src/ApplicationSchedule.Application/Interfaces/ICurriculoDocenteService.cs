using ApplicationSchedule.Application.DTOs.Curriculos;
using ApplicationSchedule.Application.DTOs.Disponibilidades;

namespace ApplicationSchedule.Application.Interfaces;

/// <summary>
/// Define operaciones para cargar currículos docentes y consultar asignaturas habilitadas.
/// </summary>
public interface ICurriculoDocenteService
{
    Task<ImportarCurriculoResponse> ImportarDesdeExcelAsync(
        Stream archivo,
        string nombreArchivo,
        CancellationToken cancellationToken = default
    );

    Task<List<AsignaturaHabilitadaDocenteResponse>> ObtenerAsignaturasHabilitadasAsync(
        string idDocente,
        CancellationToken cancellationToken = default
    );

    Task<List<DisponibilidadDocenteResponse>> ObtenerDisponibilidadDocenteAsync(
        string idDocente,
        CancellationToken cancellationToken = default
    );
    Task<ReduccionDisponibilidadResponse> ReducirDisponibilidadPorDobleJornadaAsync(
        string idDocente,
        string idAsignatura,
        string periodo,
        CancellationToken cancellationToken = default
    );

    Task<AsignaturaHabilitadaDocenteResponse> HabilitarAsignaturaAsync(
        string idDocente,
        string idAsignatura,
        CancellationToken cancellationToken = default
    );

    Task DesvincularAsignaturaAsync(
        string idDocente,
        string idAsignatura,
        CancellationToken cancellationToken = default
    );
}