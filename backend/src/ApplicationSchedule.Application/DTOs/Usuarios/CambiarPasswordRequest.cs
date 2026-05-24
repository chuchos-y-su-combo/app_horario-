using System.ComponentModel.DataAnnotations;

namespace ApplicationSchedule.Application.DTOs.Usuarios;

/// <summary>
/// DTO para solicitar el cambio de contraseña de un usuario.
/// Contiene la nueva contraseña en texto plano (será validada y hasheada por el servicio).
/// </summary>
public class CambiarPasswordRequest
{
    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La nueva contraseña debe tener mínimo 8 caracteres.")]
    public string NuevaPassword { get; set; } = string.Empty;
}