namespace ApplicationSchedule.Domain.Entities;

/// <summary>
/// Entidad que representa un rol de seguridad/permiso dentro del sistema.
/// </summary>
public class Rol
{
    /// <summary>
    /// Identificador numérico del rol.
    /// </summary>
    public int IdRol { get; set; }

    /// <summary>
    /// Nombre legible del rol (ej. Administrador, Usuario).
    /// </summary>
    public string NombreRol { get; set; } = string.Empty;

    /// <summary>
    /// Usuarios asociados al rol.
    /// </summary>
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}