namespace ApplicationSchedule.Application.DTOs.Horarios;

/// <summary>
/// DTO que expone un calendario semanal consolidado con casillas por día/hora.
/// </summary>
public class CalendarioSemanalResponse
{
	public string Semestre { get; set; } = string.Empty;

	public string? IdPlanFiltro { get; set; }

	public string? JornadaFiltro { get; set; }

	/// <summary>
	/// Columnas del calendario: Lunes a Sábado (días 1-6).
	/// Cada columna tiene la lista de bloques asignados ese día.
	/// </summary>
	public List<DiaSemanaCalendario> Dias { get; set; } = new();
}

public class DiaSemanaCalendario
{
	/// <summary>
	/// Número de día de la semana (1 = Lunes, 6 = Sábado).
	/// </summary>
	public int NumeroDia { get; set; }

	/// <summary>
	/// Nombre legible del día.
	/// </summary>
	public string NombreDia { get; set; } = string.Empty;

	/// <summary>
	/// Bloques horarios del día ordenados por hora de inicio.
	/// </summary>
	public List<BloqueCalendario> Bloques { get; set; } = new();
}

public class BloqueCalendario
{
	/// <summary>
	/// Identificador de la asignación asociada al bloque.
	/// </summary>
	public string IdAsignacion { get; set; } = string.Empty;

	/// <summary>
	/// Hora de inicio del bloque.
	/// </summary>
	public string HoraInicio { get; set; } = string.Empty;

	/// <summary>
	/// Hora de fin del bloque.
	/// </summary>
	public string HoraFin { get; set; } = string.Empty;

	/// <summary>
	/// Nombre de la asignatura.
	/// </summary>
	public string NombreAsignatura { get; set; } = string.Empty;

	/// <summary>
	/// Código de la asignatura.
	/// </summary>
	public string CodigoAsignatura { get; set; } = string.Empty;

	/// <summary>
	/// Nombre del docente asignado.
	/// </summary>
	public string NombreDocente { get; set; } = string.Empty;

	/// <summary>
	/// Escenario de generación al que pertenece el bloque.
	/// </summary>
	public string Escenario { get; set; } = string.Empty;

	/// <summary>
	/// Jornada asociada al bloque.
	/// </summary>
	public string Jornada { get; set; } = string.Empty;

	/// <summary>
	/// Nombre del plan de estudios.
	/// </summary>
	public string NombrePlan { get; set; } = string.Empty;

	/// <summary>
	/// Identificador del plan de estudios.
	/// </summary>
	public string IdPlan { get; set; } = string.Empty;

	/// <summary>
	/// Estado de la asignación.
	/// </summary>
	public string Estado { get; set; } = string.Empty;
}