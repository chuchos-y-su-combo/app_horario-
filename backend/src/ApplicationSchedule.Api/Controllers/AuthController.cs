using ApplicationSchedule.Application.DTOs.Auth;
using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using BCrypt.Net;

namespace ApplicationSchedule.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/auth")]
/// <summary>
/// Controlador de autenticación.
/// </summary>
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly AppDbContext _context;

    private static readonly ConcurrentDictionary<string, (string Codigo, DateTime Expira)> _codigosRecuperacion = new(StringComparer.OrdinalIgnoreCase);

    public AuthController(IAuthService authService, AppDbContext context)
    {
        _authService = authService;
        _context = context;
    }

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

    [HttpPost("solicitar-recuperacion")]
    public async Task<IActionResult> SolicitarRecuperacion([FromBody] SolicitarRecuperacionRequest request)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Correo == request.Correo);

        if (usuario is null)
        {
            return Ok(new { mensaje = "Si el correo existe, se ha enviado un código de recuperación." });
        }

        string codigo = Random.Shared.Next(100000, 999999).ToString();
        _codigosRecuperacion[request.Correo] = (codigo, DateTime.UtcNow.AddMinutes(15));

        return Ok(new { mensaje = "Si el correo existe, se ha enviado un código de recuperación." });
    }

    [HttpPost("verificar-codigo")]
    public IActionResult VerificarCodigo([FromBody] VerificarCodigoRequest request)
    {
        if (!_codigosRecuperacion.TryGetValue(request.Correo, out var entrada))
        {
            return BadRequest(new { mensaje = "No hay un código de recuperación activo para este correo." });
        }

        if (DateTime.UtcNow > entrada.Expira)
        {
            _codigosRecuperacion.TryRemove(request.Correo, out _);
            return BadRequest(new { mensaje = "El código de verificación ha expirado. Por favor solicite uno nuevo." });
        }

        if (entrada.Codigo != request.Codigo)
        {
            return BadRequest(new { mensaje = "El código de verificación es incorrecto." });
        }

        return Ok(new { mensaje = "Código verificado correctamente." });
    }

    [HttpPost("cambiar-contrasena")]
    public async Task<IActionResult> CambiarContrasena([FromBody] CambiarContrasenaRequest request)
    {
        if (!_codigosRecuperacion.TryGetValue(request.Correo, out var entrada))
        {
            return BadRequest(new { mensaje = "No hay un código de recuperación activo para este correo." });
        }

        if (DateTime.UtcNow > entrada.Expira)
        {
            _codigosRecuperacion.TryRemove(request.Correo, out _);
            return BadRequest(new { mensaje = "El código de verificación ha expirado." });
        }

        if (entrada.Codigo != request.Codigo)
        {
            return BadRequest(new { mensaje = "El código de verificación es incorrecto." });
        }

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Correo == request.Correo);

        if (usuario is null)
        {
            return BadRequest(new { mensaje = "Usuario no encontrado." });
        }

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NuevaContrasena, workFactor: 11);
        await _context.SaveChangesAsync();

        _codigosRecuperacion.TryRemove(request.Correo, out _);

        return Ok(new { mensaje = "Contraseña actualizada correctamente." });
    }
}

public class SolicitarRecuperacionRequest
{
    public string Correo { get; set; } = string.Empty;
}

public class VerificarCodigoRequest
{
    public string Correo { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
}

public class CambiarContrasenaRequest
{
    public string Correo { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string NuevaContrasena { get; set; } = string.Empty;
}
