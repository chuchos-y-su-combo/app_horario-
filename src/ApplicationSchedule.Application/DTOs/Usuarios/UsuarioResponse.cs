namespace ApplicationSchedule.Application.DTOs.Usuarios;

public class UsuarioResponse
{
    public string IdUsuario { get; set; } = string.Empty;

    public string NombreCompleto { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public int IdRol { get; set; }

    public string NombreRol { get; set; } = string.Empty;
}