namespace ApplicationSchedule.Domain.Entities;

/// <summary>
/// Entidad del dominio que representa un usuario del sistema.
/// Contiene los campos persistidos por EF Core y las relaciones relevantes.
/// </summary>
public class Usuario
{
    /// <summary>
    /// Identificador único del usuario (generado por defecto como GUID string).
    /// </summary>
    public string IdUsuario { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Identificador del rol asociado (clave foránea).
    /// </summary>
    public int IdRol { get; set; }

    /// <summary>
    /// Correo electrónico utilizado para autenticación y comunicación.
    /// </summary>
    public string Correo { get; set; } = string.Empty;

    /// <summary>
    /// Hash de la contraseña (no almacenar contraseñas en texto claro).
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Nombre completo del usuario.
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// Navegación hacia la entidad Rol (puede ser null si no se carga).
    /// </summary>
    public Rol? Rol { get; set; }
}