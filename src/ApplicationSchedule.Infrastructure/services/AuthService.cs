using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApplicationSchedule.Application.DTOs.Auth;
using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ApplicationSchedule.Infrastructure.Services;

/// <summary>
/// Servicio de autenticación que valida credenciales y emite JSON Web Tokens (JWT).
/// Utiliza <see cref="AppDbContext"/> para recuperar usuarios y <see cref="IConfiguration"/> para leer claves JWT.
/// </summary>
public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Crea una instancia de <see cref="AuthService"/> con contexto y configuración inyectados.
    /// </summary>
    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Valida credenciales y devuelve un <see cref="LoginResponse"/> con token JWT.
    /// Lanza <see cref="UnauthorizedAccessException"/> si las credenciales son inválidas.
    /// </summary>
    /// <param name="request">DTO con correo y contraseña.</param>
    /// <returns>Información de sesión incluyendo token y fecha de expiración.</returns>
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        string correoNormalizado = request.Correo.Trim().ToLower();

        Usuario? usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Correo == correoNormalizado);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
        {
            throw new UnauthorizedAccessException("Credenciales incorrectas.");
        }

        string nombreRol = usuario.Rol?.NombreRol
            ?? throw new InvalidOperationException("El usuario no tiene un rol asignado.");

        string token = GenerarToken(usuario, nombreRol);

        DateTime expiracion = DateTime.UtcNow.AddMinutes(60);

        return new LoginResponse
        {
            Token = token,
            IdUsuario = usuario.IdUsuario,
            NombreCompleto = usuario.NombreCompleto,
            Correo = usuario.Correo,
            Rol = nombreRol,
            Expiracion = expiracion
        };
    }

    /// <summary>
    /// Genera un token JWT firmado con la clave configurada en `Jwt:SecretKey`.
    /// Incluye reclamos estándar y de rol. Lanza <see cref="InvalidOperationException"/> si la clave no está configurada.
    /// </summary>
    /// <param name="usuario">Entidad de usuario.</param>
    /// <param name="nombreRol">Nombre del rol del usuario.</param>
    /// <returns>Token JWT en formato compactado (string).</returns>
    private string GenerarToken(Usuario usuario, string nombreRol)
    {
        string secretKey = _configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("La clave no está configurada.");

        string issuer = _configuration["Jwt:Issuer"] ?? "ApplicationSchedule";
        string audience = _configuration["Jwt:Audience"] ?? "ApplicationScheduleClients";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.IdUsuario),
            new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario),

            new Claim(JwtRegisteredClaimNames.Email, usuario.Correo),
            new Claim(ClaimTypes.Email, usuario.Correo),

            new Claim(ClaimTypes.Name, usuario.NombreCompleto),

            new Claim(ClaimTypes.Role, nombreRol),
            new Claim("idRol", usuario.IdRol.ToString()),

            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}