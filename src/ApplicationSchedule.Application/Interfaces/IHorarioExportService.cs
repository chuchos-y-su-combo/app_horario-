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
    Task<byte[]> ExportarHorariosAsync(int? semestre, string? idDocente, string? idAsignatura, string? periodo);
}
