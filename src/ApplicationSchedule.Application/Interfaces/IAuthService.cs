using ApplicationSchedule.Application.DTOs.Auth;

namespace ApplicationSchedule.Application.Interfaces;

/// <summary>
/// Servicio de autenticación encargado de validar credenciales y emitir tokens.
/// Define el contrato para las operaciones de autenticación utilizadas por la API.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Valida las credenciales del usuario y devuelve la información de sesión.
    /// </summary>
    /// <param name="request">DTO con correo y contraseña.</param>
    /// <returns><see cref="LoginResponse"/> que contiene el token JWT y datos asociados.</returns>
    Task<LoginResponse> LoginAsync(LoginRequest request);
}
