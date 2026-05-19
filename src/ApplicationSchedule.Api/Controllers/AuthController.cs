using ApplicationSchedule.Application.DTOs.Auth;
using ApplicationSchedule.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ApplicationSchedule.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/auth")]
/// <summary>
/// Controlador de autenticación.
/// Expone el endpoint de inicio de sesión y delega la validación de credenciales a <see cref="IAuthService"/>.
/// </summary>
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Constructor de <see cref="AuthController"/>.
    /// </summary>
    /// <param name="authService">Servicio de autenticación inyectado.</param>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Valida las credenciales proporcionadas y devuelve un token JWT en caso de éxito.
    /// </summary>
    /// <param name="request">Datos de inicio de sesión (correo y contraseña).</param>
    /// <returns><see cref="LoginResponse"/> con el token y datos de sesión; 401 si credenciales inválidas.</returns>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        try
        {
            LoginResponse response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { mensaje = "Correo o contraseña incorrectos." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
