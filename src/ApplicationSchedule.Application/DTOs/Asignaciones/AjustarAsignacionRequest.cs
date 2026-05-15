using System.ComponentModel.DataAnnotations;

namespace ApplicationSchedule.Application.DTOs.Asignaciones;

public class AjustarAsignacionRequest
{
	public string? IdDocente { get; set; }

	public string? IdAsignatura { get; set; }

	[Range(1, 6, ErrorMessage = "El día debe estar entre 1 y 6.")]
	public int? Dia { get; set; }

	[RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "La hora de inicio debe tener formato HH:mm.")]
	public string? HoraInicio { get; set; }

	[RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "La hora de fin debe tener formato HH:mm.")]
	public string? HoraFin { get; set; }

	public string? Periodo { get; set; }
}