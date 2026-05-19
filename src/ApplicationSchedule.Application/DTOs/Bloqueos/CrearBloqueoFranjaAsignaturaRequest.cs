using System.ComponentModel.DataAnnotations;

namespace ApplicationSchedule.Application.DTOs.Bloqueos;

/// <summary>
/// Datos necesarios para bloquear una franja horaria de una asignatura.
/// </summary>
/// <summary>
/// DTO para crear un bloqueo de franja horario para una asignatura.
/// Utilizado para marcar franjas donde no debe programarse la asignatura.
/// </summary>
public class CrearBloqueoFranjaAsignaturaRequest
{
    [Required(ErrorMessage = "El periodo es obligatorio.")]
    [MaxLength(10, ErrorMessage = "El periodo no puede superar los 10 caracteres.")]
    public string Periodo { get; set; } = string.Empty;

    /// <summary>
    /// 1 = Lunes, 2 = Martes, 3 = Miércoles, 4 = Jueves, 5 = Viernes, 6 = Sábado.
    /// </summary>
    [Range(1, 6, ErrorMessage = "El día debe estar entre 1 (Lunes) y 6 (Sábado).")]
    public int Dia { get; set; }

    /// <summary>
    /// Formato esperado HH:mm, por ejemplo 08:00.
    /// </summary>
    [Required(ErrorMessage = "La hora de inicio es obligatoria.")]
    [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "La hora de inicio debe tener formato HH:mm.")]
    public string HoraInicio { get; set; } = string.Empty;

    /// <summary>
    /// Formato esperado HH:mm, por ejemplo 10:00.
    /// </summary>
    [Required(ErrorMessage = "La hora de fin es obligatoria.")]
    [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "La hora de fin debe tener formato HH:mm.")]
    public string HoraFin { get; set; } = string.Empty;

    [MaxLength(250, ErrorMessage = "El motivo no puede superar los 250 caracteres.")]
    public string? Motivo { get; set; }
}