using BCrypt.Net;
using ApplicationSchedule.Application.DTOs.Usuarios;
using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ApplicationSchedule.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly AppDbContext _context;

    public UsuarioService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UsuarioResponse>> ObtenerTodosAsync()
    {
        return await _context.Usuarios
            .Include(u => u.Rol)
            .OrderBy(u => u.NombreCompleto)
            .Select(u => new UsuarioResponse
            {
                IdUsuario = u.IdUsuario,
                NombreCompleto = u.NombreCompleto,
                Correo = u.Correo,
                IdRol = u.IdRol,
                NombreRol = u.Rol != null ? u.Rol.NombreRol : string.Empty
            })
            .ToListAsync();
    }

    public async Task<UsuarioResponse?> ObtenerPorIdAsync(string idUsuario)
    {
        return await _context.Usuarios
            .Include(u => u.Rol)
            .Where(u => u.IdUsuario == idUsuario)
            .Select(u => new UsuarioResponse
            {
                IdUsuario = u.IdUsuario,
                NombreCompleto = u.NombreCompleto,
                Correo = u.Correo,
                IdRol = u.IdRol,
                NombreRol = u.Rol != null ? u.Rol.NombreRol : string.Empty
            })
            .FirstOrDefaultAsync();
    }

    public async Task<UsuarioResponse> CrearAsync(CrearUsuarioRequest request)
    {
        string correoNormalizado = request.Correo.Trim().ToLower();

        bool correoExiste = await _context.Usuarios
            .AnyAsync(u => u.Correo.ToLower() == correoNormalizado);

        if (correoExiste)
        {
            throw new InvalidOperationException("Ya existe un usuario registrado con ese correo.");
        }

        bool rolExiste = await _context.Roles
            .AnyAsync(r => r.IdRol == request.IdRol);

        if (!rolExiste)
        {
            throw new InvalidOperationException("El rol seleccionado no existe.");
        }

        var usuario = new Usuario
        {
            IdUsuario = Guid.NewGuid().ToString(),
            NombreCompleto = request.NombreCompleto.Trim(),
            Correo = correoNormalizado,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IdRol = request.IdRol
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        UsuarioResponse? usuarioCreado = await ObtenerPorIdAsync(usuario.IdUsuario);

        if (usuarioCreado is null)
        {
            throw new InvalidOperationException("No se pudo recuperar el usuario creado.");
        }

        return usuarioCreado;
    }

    public async Task<bool> ActualizarAsync(string idUsuario, ActualizarUsuarioRequest request)
    {
        Usuario? usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

        if (usuario is null)
        {
            return false;
        }

        string correoNormalizado = request.Correo.Trim().ToLower();

        bool correoUsadoPorOtroUsuario = await _context.Usuarios
            .AnyAsync(u => u.Correo.ToLower() == correoNormalizado && u.IdUsuario != idUsuario);

        if (correoUsadoPorOtroUsuario)
        {
            throw new InvalidOperationException("El correo ya está siendo usado por otro usuario.");
        }

        bool rolExiste = await _context.Roles
            .AnyAsync(r => r.IdRol == request.IdRol);

        if (!rolExiste)
        {
            throw new InvalidOperationException("El rol seleccionado no existe.");
        }

        usuario.NombreCompleto = request.NombreCompleto.Trim();
        usuario.Correo = correoNormalizado;
        usuario.IdRol = request.IdRol;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CambiarPasswordAsync(string idUsuario, CambiarPasswordRequest request)
    {
        Usuario? usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

        if (usuario is null)
        {
            return false;
        }

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NuevaPassword);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> EliminarAsync(string idUsuario)
    {
        Usuario? usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

        if (usuario is null)
        {
            return false;
        }

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();

        return true;
    }
}