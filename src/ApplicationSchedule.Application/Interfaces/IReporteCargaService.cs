using ApplicationSchedule.Application.DTOs.Horarios;

namespace ApplicationSchedule.Application.Interfaces;

/// <summary>
/// Servicio encargado de generar reportes de carga docente y por docente.
/// </summary>
public interface IReporteCargaService
{
	/// <summary>
	/// Genera el reporte de carga docente para un semestre completo.
	/// </summary>
	Task<ReporteCargaDocenteResponse> GenerarReportePorSemestreAsync(
		string semestre,
		CancellationToken cancellationToken = default
	);

	/// <summary>
	/// Genera un reporte individual por docente para el semestre indicado.
	/// </summary>
	Task<ReporteDocenteItem> GenerarReportePorDocenteAsync(
		string idDocente,
		string semestre,
		CancellationToken cancellationToken = default
	);
}