using System.ComponentModel.DataAnnotations;

namespace ApplicationSchedule.Application.DTOs.Asignaciones;

public class ConfirmarAsignacionesRequest
{
    [Required(ErrorMessage = "Debe enviar al menos un ID de asignación.")]
    [MinLength(1, ErrorMessage = "Debe enviar al menos un ID de asignación.")]
    public List<string> IdsAsignacion { get; set; } = new();
}