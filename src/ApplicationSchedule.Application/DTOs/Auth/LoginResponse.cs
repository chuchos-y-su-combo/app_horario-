namespace ApplicationSchedule.Application.DTOs.Auth;

/// <summary>
/// DTO devuelto tras un login exitoso. Contiene el token JWT y datos básicos del usuario.
/// </summary>
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string IdUsuario { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public DateTime Expiracion { get; set; }
}
