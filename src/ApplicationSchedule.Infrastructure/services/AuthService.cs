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

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        string correoNormalizado = request.Correo.Trim().ToLower();
        Usuario? usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Correo == correoNormalizado);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
            throw new UnauthorizedAccessException("Credenciales incorrectas.");

        string token = GenerarToken(usuario);
        DateTime expiracion = DateTime.UtcNow.AddMinutes(60);

        return new LoginResponse
        {
            Token = token,
            IdUsuario = usuario.IdUsuario,
            NombreCompleto = usuario.NombreCompleto,
            Correo = usuario.Correo,
            Rol = string.Empty,
            Expiracion = expiracion
        };
    }

    private string GenerarToken(Usuario usuario)
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
            new Claim(JwtRegisteredClaimNames.Email, usuario.Correo),
            new Claim(ClaimTypes.Name, usuario.NombreCompleto),
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