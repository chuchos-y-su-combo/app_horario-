using System.ComponentModel.DataAnnotations;

namespace ApplicationSchedule.Application.DTOs.Asignaciones;

/// <summary>
/// DTO que transporta una lista de identificadores de asignaciones a confirmar.
/// Utilizado para pasar de propuestas a asignaciones definitivas.
/// </summary>
public class ConfirmarAsignacionesRequest
{
    [Required(ErrorMessage = "Debe enviar al menos un ID de asignación.")]
    [MinLength(1, ErrorMessage = "Debe enviar al menos un ID de asignación.")]
    public List<string> IdsAsignacion { get; set; } = new();
}