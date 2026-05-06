using System.ComponentModel.DataAnnotations;

namespace ApplicationSchedule.Application.DTOs.Usuarios;

public class CambiarPasswordRequest
{
    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La nueva contraseña debe tener mínimo 8 caracteres.")]
    public string NuevaPassword { get; set; } = string.Empty;
}