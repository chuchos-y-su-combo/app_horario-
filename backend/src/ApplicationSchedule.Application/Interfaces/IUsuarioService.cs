using ApplicationSchedule.Application.DTOs.Usuarios;

namespace ApplicationSchedule.Application.Interfaces;

/// <summary>
/// Servicio de aplicación que encapsula las operaciones relacionadas con usuarios.
/// Define los contratos que la capa API y otras capas utilizarán para gestionar usuarios.
/// </summary>
public interface IUsuarioService
{
    /// <summary>
    /// Recupera todos los usuarios registrados.
    /// </summary>
    Task<List<UsuarioResponse>> ObtenerTodosAsync();

    /// <summary>
    /// Obtiene un usuario por su identificador.
    /// </summary>
    /// <param name="idUsuario">Identificador del usuario.</param>
    /// <returns>DTO de usuario o null si no existe.</returns>
    Task<UsuarioResponse?> ObtenerPorIdAsync(string idUsuario);

    /// <summary>
    /// Crea un nuevo usuario en el sistema.
    /// </summary>
    /// <param name="request">Datos para la creación del usuario.</param>
    /// <returns>DTO con la información del usuario creado.</returns>
    Task<UsuarioResponse> CrearAsync(CrearUsuarioRequest request);

    /// <summary>
    /// Actualiza los datos de un usuario existente.
    /// </summary>
    /// <param name="idUsuario">Identificador del usuario a actualizar.</param>
    /// <param name="request">Datos con los campos a modificar.</param>
    /// <returns>True si se actualizó; false si no se encontró el usuario.</returns>
    Task<bool> ActualizarAsync(string idUsuario, ActualizarUsuarioRequest request);

    /// <summary>
    /// Cambia la contraseña de un usuario.
    /// </summary>
    /// <param name="idUsuario">Identificador del usuario.</param>
    /// <param name="request">Datos con la contraseña actual y la nueva.</param>
    /// <returns>True si el cambio fue exitoso; false si no se encontró el usuario.</returns>
    Task<bool> CambiarPasswordAsync(string idUsuario, CambiarPasswordRequest request);

    /// <summary>
    /// Elimina un usuario del sistema.
    /// </summary>
    /// <param name="idUsuario">Identificador del usuario a eliminar.</param>
    /// <returns>True si se eliminó; false si no se encontró.</returns>
    Task<bool> EliminarAsync(string idUsuario);
}