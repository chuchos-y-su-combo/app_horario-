using ApplicationSchedule.Application.DTOs.Horarios;

namespace ApplicationSchedule.Application.Interfaces;

/// <summary>
/// Contrato para servicios que exportan horarios a formatos binarios (ej. Excel).
/// Implementaciones deben devolver el contenido del archivo como array de bytes.
/// </summary>
public interface IHorarioExportService
{
    /// <summary>
    /// Exporta horarios filtrados a un archivo y devuelve su contenido en bytes.
    /// </summary>
    /// <param name="semestre">Filtro por semestre académico de la asignatura (1-12).</param>
    /// <param name="idDocente">Filtro por docente; "__ALL__" genera una hoja por cada docente.</param>
    /// <param name="idAsignatura">Filtro por asignatura específica.</param>
    /// <param name="idPlan">Filtro por plan de estudios.</param>
    /// <param name="periodo">Período académico (ej. "2026-1").</param>
    /// <param name="porSemestre">Cuando es true y hay un idPlan, genera una hoja por semestre del plan.</param>
    Task<byte[]> ExportarHorariosAsync(
        int? semestre,
        string? idDocente,
        string? idAsignatura,
        string? idPlan,
        string? periodo,
        bool porSemestre = false);
}
