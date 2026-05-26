using ApplicationSchedule.Application.DTOs.Asignaciones;

namespace ApplicationSchedule.Application.Interfaces;

public interface IConflictoAsignacionService
{
	/// <summary>
	/// Analiza todas las asignaciones del semestre y devuelve
	/// los conflictos encontrados: cruces horarios, exceso de carga,
	/// asignaturas sin docente y docentes con asignaturas sin bloque horario.
	/// </summary>
	Task<ConflictoAsignacionResponse> AnalizarConflictosAsync(
		string semestre,
		CancellationToken cancellationToken = default
	);
}