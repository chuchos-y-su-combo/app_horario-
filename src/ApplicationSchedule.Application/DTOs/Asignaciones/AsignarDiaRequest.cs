using System.ComponentModel.DataAnnotations;

namespace ApplicationSchedule.Application.DTOs.Asignaciones;

/// <summary>
/// Permite colocar una asignación existente en un día de la semana.
/// La hora es opcional: si no se envía, el bloque queda en el día
/// pero sin hora definida y no aparece en el calendario hasta completarla.
/// Si se envía hora, debe enviarse tanto inicio como fin.
/// </summary>
/// <summary>
/// DTO para asignar una asignatura a un docente en un día específico (operación por día).
/// </summary>
public class AsignarDiaRequest
{
    [Required(ErrorMessage = "El día es obligatorio.")]
    [Range(1, 6, ErrorMessage = "El día debe estar entre 1 (Lunes) y 6 (Sábado).")]
    public int Dia { get; set; }

    [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$",
        ErrorMessage = "La hora de inicio debe tener formato HH:mm.")]
    public string? HoraInicio { get; set; }

    [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$",
        ErrorMessage = "La hora de fin debe tener formato HH:mm.")]
    public string? HoraFin { get; set; }
}
