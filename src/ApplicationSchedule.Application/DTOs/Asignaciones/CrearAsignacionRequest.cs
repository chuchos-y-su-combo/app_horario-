using System.ComponentModel.DataAnnotations;

namespace ApplicationSchedule.Application.DTOs.Asignaciones;

public class CrearAsignacionRequest
{
    [Required(ErrorMessage = "El docente es obligatorio.")]
    public string IdDocente { get; set; } = string.Empty;

    [Required(ErrorMessage = "La asignatura es obligatoria.")]
    public string IdAsignatura { get; set; } = string.Empty;

    [Required(ErrorMessage = "El día es obligatorio.")]
    [Range(1, 6, ErrorMessage = "El día debe estar entre 1 y 6.")]
    public int Dia { get; set; }

    [Required(ErrorMessage = "La hora de inicio es obligatoria.")]
    [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "La hora de inicio debe tener formato HH:mm.")]
    public string HoraInicio { get; set; } = string.Empty;

    [Required(ErrorMessage = "La hora de fin es obligatoria.")]
    [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "La hora de fin debe tener formato HH:mm.")]
    public string HoraFin { get; set; } = string.Empty;

    [Required(ErrorMessage = "El periodo es obligatorio.")]
    [MaxLength(10, ErrorMessage = "El periodo no puede superar los 10 caracteres.")]
    public string Periodo { get; set; } = string.Empty;
}