namespace ApplicationSchedule.Application.DTOs.Usuarios;

/// <summary>
/// DTO de salida que representa la información pública de un usuario.
/// Utilizado en respuestas de API y en capas superiores para transportar datos del usuario.
/// </summary>
public class UsuarioResponse
{
    /// <summary>
    /// Identificador único del usuario (string GUID o similar).
    /// </summary>
    public string IdUsuario { get; set; } = string.Empty;

    /// <summary>
    /// Nombre completo del usuario.
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// Correo electrónico asociado al usuario.
    /// </summary>
    public string Correo { get; set; } = string.Empty;

    /// <summary>
    /// Identificador del rol asignado al usuario.
    /// </summary>
    public int IdRol { get; set; }

    /// <summary>
    /// Nombre legible del rol (por ejemplo: Administrador, Coordinador).
    /// </summary>
    public string NombreRol { get; set; } = string.Empty;
}