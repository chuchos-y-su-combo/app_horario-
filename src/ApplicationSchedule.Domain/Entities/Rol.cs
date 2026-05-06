namespace ApplicationSchedule.Domain.Entities;

public class Rol
{
    public int IdRol { get; set; }

    public string NombreRol { get; set; } = string.Empty;

    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}