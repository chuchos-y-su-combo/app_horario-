using ApplicationSchedule.Application.DTOs.Horarios;

namespace ApplicationSchedule.Application.Interfaces;

public interface IHorarioExportService
{
    Task<byte[]> ExportarHorariosAsync(int? semestre, string? idDocente, string? idAsignatura);
}
