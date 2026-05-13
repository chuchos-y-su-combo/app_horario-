using ApplicationSchedule.Application.DTOs.Curriculos;

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
}