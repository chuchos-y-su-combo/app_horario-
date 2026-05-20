using System.ComponentModel.DataAnnotations;

namespace ApplicationSchedule.Application.DTOs.Usuarios;

/// <summary>
/// DTO para la creación de un nuevo usuario.
/// Contiene las validaciones de datos necesarias para la operación de creación.
/// </summary>
public class CrearUsuarioRequest
{
    /// <summary>
    /// Nombre completo del usuario. Máximo 150 caracteres.
    /// </summary>
    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [MaxLength(150, ErrorMessage = "El nombre completo no puede superar los 150 caracteres.")]
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// Correo electrónico del usuario. Debe ser una dirección válida y hasta 100 caracteres.
    /// </summary>
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [MaxLength(100, ErrorMessage = "El correo no puede superar los 100 caracteres.")]
    public string Correo { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña en texto plano (será hasheada por el servicio al persistir).
    /// Requiere mínimo 8 caracteres.
    /// </summary>
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener mínimo 8 caracteres.")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Identificador numérico del rol asignado al usuario.
    /// </summary>
    [Required(ErrorMessage = "El rol es obligatorio.")]
    public int IdRol { get; set; }
}