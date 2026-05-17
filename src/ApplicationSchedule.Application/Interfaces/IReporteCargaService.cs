using ApplicationSchedule.Application.DTOs.Horarios;

namespace ApplicationSchedule.Application.Interfaces;

public interface IReporteCargaService
{
	Task<ReporteCargaDocenteResponse> GenerarReportePorSemestreAsync(
		string semestre,
		CancellationToken cancellationToken = default
	);

	Task<ReporteDocenteItem> GenerarReportePorDocenteAsync(
		string idDocente,
		string semestre,
		CancellationToken cancellationToken = default
	);
}