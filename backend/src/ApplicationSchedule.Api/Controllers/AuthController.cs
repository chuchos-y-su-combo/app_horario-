using ApplicationSchedule.Application.DTOs.Auth;
using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using BCrypt.Net;

namespace ApplicationSchedule.Api.Controllers;

/// <summary>
/// Controlador de autenticación y recuperación de contraseña.
/// Todos sus endpoints son públicos (<see cref="AllowAnonymousAttribute"/>).
/// Los códigos de recuperación se almacenan en memoria con expiración de 15 minutos.
/// </summary>
[AllowAnonymous]
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly AppDbContext _context;

    /// <summary>
    /// Almacén en memoria de códigos de recuperación activos.
    /// Clave: correo del usuario (insensible a mayúsculas).
    /// Valor: código generado y su fecha de expiración.
    /// </summary>
    private static readonly ConcurrentDictionary<string, (string Codigo, DateTime Expira)> _codigosRecuperacion = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Inicializa el controlador con el servicio de autenticación y el contexto de BD.
    /// </summary>
    public AuthController(IAuthService authService, AppDbContext context)
    {
        _authService = authService;
        _context = context;
    }

    /// <summary>
    /// Autentica al usuario y devuelve un JWT si las credenciales son correctas.
    /// </summary>
    /// <returns>Token JWT, nombre completo y rol del usuario autenticado.</returns>
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

    /// <summary>
    /// Genera y almacena un código numérico de 6 dígitos para el correo indicado.
    /// La respuesta es siempre la misma para no revelar si el correo existe o no.
    /// En producción el código debe enviarse por email; actualmente queda en memoria.
    /// </summary>
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

    /// <summary>
    /// Valida que el código de recuperación recibido sea correcto y no haya expirado.
    /// Debe llamarse antes de <see cref="CambiarContrasena"/>.
    /// </summary>
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

    /// <summary>
    /// Actualiza la contraseña del usuario tras verificar que el código sea válido.
    /// Elimina el código de recuperación al finalizar para evitar reutilización.
    /// </summary>
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

/// <summary>Cuerpo de la solicitud para iniciar la recuperación de contraseña.</summary>
public class SolicitarRecuperacionRequest
{
    /// <summary>Correo institucional del usuario que olvidó su contraseña.</summary>
    public string Correo { get; set; } = string.Empty;
}

/// <summary>Cuerpo de la solicitud para verificar el código de 6 dígitos.</summary>
public class VerificarCodigoRequest
{
    /// <summary>Correo del usuario que está verificando el código.</summary>
    public string Correo { get; set; } = string.Empty;
    /// <summary>Código numérico de 6 dígitos recibido en el correo.</summary>
    public string Codigo { get; set; } = string.Empty;
}

/// <summary>Cuerpo de la solicitud para establecer una nueva contraseña.</summary>
public class CambiarContrasenaRequest
{
    /// <summary>Correo del usuario cuya contraseña se cambiará.</summary>
    public string Correo { get; set; } = string.Empty;
    /// <summary>Código de verificación válido previamente confirmado.</summary>
    public string Codigo { get; set; } = string.Empty;
    /// <summary>Nueva contraseña en texto plano (se almacena hasheada con BCrypt).</summary>
    public string NuevaContrasena { get; set; } = string.Empty;
}
