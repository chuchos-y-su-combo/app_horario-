using BCrypt.Net;
using ApplicationSchedule.Application.DTOs.Usuarios;
using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ApplicationSchedule.Application.Services;

/// <summary>
/// Implementación de <see cref="IUsuarioService"/> que opera sobre <see cref="AppDbContext"/>.
/// Encapsula las operaciones CRUD para la entidad <see cref="Usuario"/> y las reglas de validación
/// relacionadas con correos y roles.
/// </summary>
public class UsuarioService : IUsuarioService
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Crea una instancia de <see cref="UsuarioService"/> con el contexto de datos inyectado.
    /// </summary>
    public UsuarioService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Recupera todos los usuarios con su rol asociado, ordenados por nombre.
    /// </summary>
    /// <returns>Lista de <see cref="UsuarioResponse"/>.</returns>
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

    /// <summary>
    /// Obtiene un usuario por su identificador.
    /// </summary>
    /// <param name="idUsuario">Identificador del usuario.</param>
    /// <returns>DTO del usuario o null si no existe.</returns>
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

    /// <summary>
    /// Crea un nuevo usuario tras validar unicidad de correo y existencia del rol.
    /// </summary>
    /// <param name="request">Datos para la creación del usuario.</param>
    /// <returns>DTO del usuario creado.</returns>
    /// <exception cref="InvalidOperationException">Si el correo ya existe o el rol no existe.</exception>
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

    /// <summary>
    /// Actualiza los datos de un usuario existente después de validaciones pertinentes.
    /// </summary>
    /// <param name="idUsuario">Identificador del usuario a actualizar.</param>
    /// <param name="request">Datos a actualizar.</param>
    /// <returns>True si se actualizó; false si no se encontró el usuario.</returns>
    /// <exception cref="InvalidOperationException">Si el correo está en uso por otro usuario o el rol no existe.</exception>
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

    /// <summary>
    /// Cambia la contraseña de un usuario, sobrescribiendo el hash almacenado.
    /// </summary>
    /// <param name="idUsuario">Identificador del usuario.</param>
    /// <param name="request">DTO con la nueva contraseña.</param>
    /// <returns>True si se cambió; false si no se encontró el usuario.</returns>
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

    /// <summary>
    /// Elimina un usuario del sistema.
    /// </summary>
    /// <param name="idUsuario">Identificador del usuario a eliminar.</param>
    /// <returns>True si se eliminó; false si no se encontró.</returns>
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