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
	public int NumeroDia { get; set; }

	public string NombreDia { get; set; } = string.Empty;

	/// <summary>
	/// Bloques horarios del día ordenados por hora de inicio.
	/// </summary>
	public List<BloqueCalendario> Bloques { get; set; } = new();
}

public class BloqueCalendario
{
	public string IdAsignacion { get; set; } = string.Empty;

	public string HoraInicio { get; set; } = string.Empty;

	public string HoraFin { get; set; } = string.Empty;

	public string NombreAsignatura { get; set; } = string.Empty;

	public string CodigoAsignatura { get; set; } = string.Empty;

	public string NombreDocente { get; set; } = string.Empty;

	public string Escenario { get; set; } = string.Empty;

	public string Jornada { get; set; } = string.Empty;

	public string NombrePlan { get; set; } = string.Empty;

	public string IdPlan { get; set; } = string.Empty;

	public string Estado { get; set; } = string.Empty;
}