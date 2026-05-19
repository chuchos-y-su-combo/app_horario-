namespace ApplicationSchedule.Application.DTOs.Auth;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// DTO para el inicio de sesión. Contiene las credenciales del usuario necesarias
/// para obtener un token JWT.
/// </summary>
public class LoginRequest
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Password { get; set; } = string.Empty;
}
