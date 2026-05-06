namespace ApplicationSchedule.Domain.Entities;

public class Usuario
{
    public string IdUsuario { get; set; } = Guid.NewGuid().ToString();

    public int IdRol { get; set; }

    public string Correo { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string NombreCompleto { get; set; } = string.Empty;

    public Rol? Rol { get; set; }
}